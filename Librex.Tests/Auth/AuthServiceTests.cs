using Librex.Application.DTOs.Auth;
using Librex.Application.UseCases.Auth;
using Librex.Domain.Entities;
using Librex.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Librex.Tests.Auth;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly IConfiguration _config;
    private readonly AuthService _sut;

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

        _sut = new AuthService(_userRepo.Object, _config);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsTokenResponse()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("Admin1234");
        _userRepo.Setup(r => r.GetByUsernameAsync("admin"))
            .ReturnsAsync(new User
            {
                Id = 1, Username = "admin", PasswordHash = hash,
                FullName = "System Administrator", Role = "Administrator",
                IsActive = true, TenantId = 1,
            });

        var result = await _sut.LoginAsync(new LoginDto { Username = "admin", Password = "Admin1234" });

        Assert.NotNull(result);
        Assert.Equal("admin", result.Username);
        Assert.Equal("System Administrator", result.FullName);
        Assert.Equal("Administrator", result.Role);
        Assert.False(string.IsNullOrEmpty(result.Token));
        Assert.True(result.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsNull()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("Admin1234");
        _userRepo.Setup(r => r.GetByUsernameAsync("admin"))
            .ReturnsAsync(new User
            {
                Id = 1, Username = "admin", PasswordHash = hash,
                FullName = "Admin", Role = "Administrator", IsActive = true, TenantId = 1,
            });

        var result = await _sut.LoginAsync(new LoginDto { Username = "admin", Password = "WrongPassword" });

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ReturnsNull()
    {
        _userRepo.Setup(r => r.GetByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        var result = await _sut.LoginAsync(new LoginDto { Username = "ghost", Password = "anything" });

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_InactiveUser_ReturnsNull()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("Admin1234");
        _userRepo.Setup(r => r.GetByUsernameAsync("disabled"))
            .ReturnsAsync(new User
            {
                Id = 2, Username = "disabled", PasswordHash = hash,
                FullName = "Disabled User", Role = "Administrator", IsActive = false, TenantId = 1,
            });

        var result = await _sut.LoginAsync(new LoginDto { Username = "disabled", Password = "Admin1234" });

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_TokenContainsExpectedClaims()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("Pass123");
        _userRepo.Setup(r => r.GetByUsernameAsync("testuser"))
            .ReturnsAsync(new User
            {
                Id = 5, Username = "testuser", PasswordHash = hash,
                FullName = "Test User", Role = "Staff", IsActive = true, TenantId = 1,
            });

        var result = await _sut.LoginAsync(new LoginDto { Username = "testuser", Password = "Pass123" });

        Assert.NotNull(result);
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.Token);
        Assert.Equal("testuser", token.Claims.First(c => c.Type == "unique_name").Value);
        Assert.Equal("Staff", token.Claims.First(c => c.Type == System.Security.Claims.ClaimTypes.Role).Value);
    }
}
