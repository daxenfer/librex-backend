using Librex.Application.DTOs.Auth;
using Librex.Application.UseCases.Auth;
using Librex.Domain.Constants;
using Librex.Domain.Entities;
using Librex.Domain.Enums;
using Librex.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Librex.Tests.Auth;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<ILoginAttemptRepository> _attemptRepo = new();
    private readonly IConfiguration _config;
    private readonly AuthService _sut;

    private static readonly LoginRequestContext Context = new("127.0.0.1", "pruebas");

    public AuthServiceTests()
    {
        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "super-secret-test-key-at-least-32-chars-long!!",
                ["Jwt:Issuer"] = "LibrexAPI",
                ["Jwt:Audience"] = "LibrexClients",
                ["Jwt:ExpirationMinutes"] = "60",
            })
            .Build();

        _sut = new AuthService(_userRepo.Object, _attemptRepo.Object, _config);
    }

    private static User Existing(string username, string password, bool active = true) => new()
    {
        Id = 1,
        Username = username,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
        FullName = "System Administrator",
        Role = Roles.Administrator,
        IsActive = active,
    };

    private Task<LoginResponseDto?> LoginAsync(string username, string password)
        => _sut.LoginAsync(new LoginDto { Username = username, Password = password }, Context);

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsTokenResponse()
    {
        _userRepo.Setup(r => r.GetByUsernameAsync("admin")).ReturnsAsync(Existing("admin", "Admin1234"));

        var result = await LoginAsync("admin", "Admin1234");

        Assert.NotNull(result);
        Assert.Equal("admin", result.Username);
        Assert.Equal("System Administrator", result.FullName);
        Assert.Equal(Roles.Administrator, result.Role);
        Assert.False(string.IsNullOrEmpty(result.Token));
        Assert.True(result.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsNull()
    {
        _userRepo.Setup(r => r.GetByUsernameAsync("admin")).ReturnsAsync(Existing("admin", "Admin1234"));

        Assert.Null(await LoginAsync("admin", "WrongPassword"));
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ReturnsNull()
    {
        _userRepo.Setup(r => r.GetByUsernameAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

        Assert.Null(await LoginAsync("ghost", "anything"));
    }

    [Fact]
    public async Task LoginAsync_InactiveUser_ReturnsNull()
    {
        _userRepo.Setup(r => r.GetByUsernameAsync("disabled"))
            .ReturnsAsync(Existing("disabled", "Admin1234", active: false));

        Assert.Null(await LoginAsync("disabled", "Admin1234"));
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_TokenContainsExpectedClaims()
    {
        var user = Existing("testuser", "Pass123");
        user.Role = Roles.User;
        _userRepo.Setup(r => r.GetByUsernameAsync("testuser")).ReturnsAsync(user);

        var result = await LoginAsync("testuser", "Pass123");

        Assert.NotNull(result);
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.Token);
        Assert.Equal("testuser", token.Claims.First(c => c.Type == "unique_name").Value);
        Assert.Equal(Roles.User, token.Claims.First(c => c.Type == System.Security.Claims.ClaimTypes.Role).Value);

        // Sin el sello, la revocación no funciona: un token sin él se rechaza en Program.cs.
        Assert.Equal(user.SecurityStamp, token.Claims.First(c => c.Type == SecurityClaims.Stamp).Value);
    }

    // ---- Bloqueo por cuenta ----

    [Fact]
    public async Task LoginAsync_CountsFailedAttempts()
    {
        var user = Existing("admin", "Admin1234");
        _userRepo.Setup(r => r.GetByUsernameAsync("admin")).ReturnsAsync(user);

        await LoginAsync("admin", "mala");
        await LoginAsync("admin", "mala");

        Assert.Equal(2, user.FailedLoginAttempts);
        Assert.Null(user.LockedOutUntil);
    }

    [Fact]
    public async Task LoginAsync_LocksAccountAtThreshold()
    {
        var user = Existing("admin", "Admin1234");
        _userRepo.Setup(r => r.GetByUsernameAsync("admin")).ReturnsAsync(user);

        for (var i = 0; i < LockoutPolicy.MaxFailedAttempts; i++)
            await LoginAsync("admin", "mala");

        Assert.NotNull(user.LockedOutUntil);
        Assert.True(user.LockedOutUntil > DateTime.UtcNow);
        Assert.Equal(0, user.FailedLoginAttempts);   // el bloqueo sustituye al contador
    }

    // Lo que hace que el bloqueo sirva: mientras dura, ni la contraseña buena entra.
    [Fact]
    public async Task LoginAsync_WhileLockedOut_RejectsEvenTheRightPassword()
    {
        var user = Existing("admin", "Admin1234");
        user.LockedOutUntil = DateTime.UtcNow.AddMinutes(5);
        _userRepo.Setup(r => r.GetByUsernameAsync("admin")).ReturnsAsync(user);

        Assert.Null(await LoginAsync("admin", "Admin1234"));
        _attemptRepo.Verify(r => r.AddAsync(It.Is<LoginAttempt>(a => a.Outcome == LoginOutcome.LockedOut)), Times.Once);
    }

    // El bloqueo es temporal: pasada la ventana se puede volver a entrar sin intervención.
    [Fact]
    public async Task LoginAsync_AfterLockoutExpires_LetsTheUserBackIn()
    {
        var user = Existing("admin", "Admin1234");
        user.LockedOutUntil = DateTime.UtcNow.AddMinutes(-1);
        user.FailedLoginAttempts = 4;
        _userRepo.Setup(r => r.GetByUsernameAsync("admin")).ReturnsAsync(user);

        Assert.NotNull(await LoginAsync("admin", "Admin1234"));
        Assert.Null(user.LockedOutUntil);
        Assert.Equal(0, user.FailedLoginAttempts);
    }

    [Fact]
    public async Task LoginAsync_SuccessAfterFailures_ResetsTheCounter()
    {
        var user = Existing("admin", "Admin1234");
        _userRepo.Setup(r => r.GetByUsernameAsync("admin")).ReturnsAsync(user);

        await LoginAsync("admin", "mala");
        await LoginAsync("admin", "mala");
        await LoginAsync("admin", "Admin1234");

        Assert.Equal(0, user.FailedLoginAttempts);
    }

    // ---- Bitácora ----

    [Theory]
    [InlineData("admin", "Admin1234", LoginOutcome.Success)]
    [InlineData("admin", "mala", LoginOutcome.BadPassword)]
    [InlineData("ghost", "loquesea", LoginOutcome.UnknownUser)]
    public async Task LoginAsync_RecordsTheRealOutcome(string username, string password, LoginOutcome expected)
    {
        _userRepo.Setup(r => r.GetByUsernameAsync("admin")).ReturnsAsync(Existing("admin", "Admin1234"));
        _userRepo.Setup(r => r.GetByUsernameAsync("ghost")).ReturnsAsync((User?)null);

        await LoginAsync(username, password);

        _attemptRepo.Verify(r => r.AddAsync(It.Is<LoginAttempt>(a =>
            a.Username == username &&
            a.Outcome == expected &&
            a.IpAddress == "127.0.0.1")), Times.Once);
    }

    // Perder un renglón de auditoría es preferible a dejar fuera a un usuario legítimo.
    [Fact]
    public async Task LoginAsync_WhenAuditWriteFails_StillLetsTheUserIn()
    {
        _userRepo.Setup(r => r.GetByUsernameAsync("admin")).ReturnsAsync(Existing("admin", "Admin1234"));
        _attemptRepo.Setup(r => r.AddAsync(It.IsAny<LoginAttempt>())).ThrowsAsync(new InvalidOperationException("bd caída"));

        Assert.NotNull(await LoginAsync("admin", "Admin1234"));
    }

    // Si el usuario no existe no debe saltarse el BCrypt: el tiempo de respuesta delataría qué
    // nombres están dados de alta. Se comprueba que un usuario inexistente no responda de
    // inmediato, que es la firma de haberse saltado la verificación.
    [Fact]
    public async Task LoginAsync_UnknownUser_StillPaysTheHashingCost()
    {
        _userRepo.Setup(r => r.GetByUsernameAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

        var started = DateTime.UtcNow;
        await LoginAsync("ghost", "loquesea");

        Assert.True(DateTime.UtcNow - started > TimeSpan.FromMilliseconds(5));
    }
}
