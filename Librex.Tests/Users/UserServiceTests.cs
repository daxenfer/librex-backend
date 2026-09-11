using Librex.Application.DTOs.Users;
using Librex.Application.UseCases.Users;
using Librex.Domain.Constants;
using Librex.Domain.Entities;
using Librex.Domain.Exceptions;
using Librex.Domain.Interfaces;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace Librex.Tests.Users;

// Las reglas de guarda son lo único con lógica real en el módulo de usuarios, y todas apuntan al
// mismo riesgo: que alguien se dé más alcance del que tiene, o deje el sistema sin quien lo
// administre.
public class UserServiceTests
{
    private readonly Mock<IUserRepository> _repo = new();
    private readonly UserService _sut;

    private static readonly ActingUser SuperAdmin = new(1, Roles.SuperAdmin);
    private static readonly ActingUser Admin = new(2, Roles.Administrator);

    public UserServiceTests()
    {
        _repo.Setup(r => r.AddAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);
        _sut = new UserService(_repo.Object, new FakeTimeProvider());
    }

    private static User Existing(int id, string role, string username = "someone") => new()
    {
        Id = id, Username = username, FullName = "Alguien", Role = role, IsActive = true,
    };

    private static CreateUserDto NewUser(string role) => new()
    {
        Username = "nuevo", FullName = "Usuario Nuevo", Role = role, Password = "Password123",
    };

    [Fact]
    public async Task CreateAsync_HashesPasswordAndNeverStoresPlainText()
    {
        User? saved = null;
        _repo.Setup(r => r.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => { saved = u; return u; });

        var result = await _sut.CreateAsync(NewUser(Roles.User), SuperAdmin);

        Assert.NotNull(saved);
        Assert.NotEqual("Password123", saved.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("Password123", saved.PasswordHash));
        Assert.Equal(Roles.User, result.Role);
    }

    [Fact]
    public async Task CreateAsync_RoleAboveActor_Throws()
    {
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _sut.CreateAsync(NewUser(Roles.SuperAdmin), Admin));

        Assert.Contains("más alcance", ex.Message);
    }

    [Fact]
    public async Task CreateAsync_UnknownRole_Throws()
        => await Assert.ThrowsAsync<BusinessRuleException>(
            () => _sut.CreateAsync(NewUser("Dueño"), SuperAdmin));

    [Fact]
    public async Task CreateAsync_UsernameTaken_Throws()
    {
        _repo.Setup(r => r.UsernameExistsAsync("nuevo", null)).ReturnsAsync(true);

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => _sut.CreateAsync(NewUser(Roles.User), SuperAdmin));
    }

    // Un usuario dado de baja sigue ocupando su nombre por el índice único de la tabla.
    [Fact]
    public async Task CreateAsync_UsernameTakenByInactiveUser_Throws()
    {
        _repo.Setup(r => r.UsernameExistsAsync(It.IsAny<string>(), It.IsAny<int?>())).ReturnsAsync(true);
        _repo.Setup(r => r.GetByUsernameAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => _sut.CreateAsync(NewUser(Roles.User), SuperAdmin));
    }

    [Fact]
    public async Task UpdateAsync_ChangingOwnRole_Throws()
    {
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(Existing(1, Roles.SuperAdmin, "yo"));

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _sut.UpdateAsync(
            1,
            new UpdateUserDto { Username = "yo", FullName = "Yo", Role = Roles.Administrator },
            SuperAdmin));

        Assert.Contains("tu propio rol", ex.Message);
    }

    [Fact]
    public async Task UpdateAsync_DemotingLastSuperAdmin_Throws()
    {
        _repo.Setup(r => r.GetByIdAsync(9)).ReturnsAsync(Existing(9, Roles.SuperAdmin, "otro"));
        _repo.Setup(r => r.CountActiveByRoleAsync(Roles.SuperAdmin)).ReturnsAsync(1);

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _sut.UpdateAsync(
            9,
            new UpdateUserDto { Username = "otro", FullName = "Otro", Role = Roles.Administrator },
            SuperAdmin));

        Assert.Contains("al menos un super administrador", ex.Message);
    }

    [Fact]
    public async Task UpdateAsync_TargetOfHigherRank_Throws()
    {
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(Existing(1, Roles.SuperAdmin));

        await Assert.ThrowsAsync<BusinessRuleException>(() => _sut.UpdateAsync(
            1,
            new UpdateUserDto { Username = "someone", FullName = "Alguien", Role = Roles.SuperAdmin },
            Admin));
    }

    [Fact]
    public async Task UpdateAsync_UnknownUser_ReturnsNull()
    {
        _repo.Setup(r => r.GetByIdAsync(404)).ReturnsAsync((User?)null);

        var result = await _sut.UpdateAsync(
            404,
            new UpdateUserDto { Username = "x", FullName = "X", Role = Roles.User },
            SuperAdmin);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_Self_Throws()
    {
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(Existing(1, Roles.SuperAdmin, "yo"));

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _sut.DeleteAsync(1, SuperAdmin));

        Assert.Contains("tu propio usuario", ex.Message);
    }

    [Fact]
    public async Task DeleteAsync_LastSuperAdmin_Throws()
    {
        _repo.Setup(r => r.GetByIdAsync(9)).ReturnsAsync(Existing(9, Roles.SuperAdmin, "otro"));
        _repo.Setup(r => r.CountActiveByRoleAsync(Roles.SuperAdmin)).ReturnsAsync(1);

        await Assert.ThrowsAsync<BusinessRuleException>(() => _sut.DeleteAsync(9, SuperAdmin));
    }

    [Fact]
    public async Task DeleteAsync_OperationalUser_DeactivatesInsteadOfErasing()
    {
        _repo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(Existing(5, Roles.User, "capturista"));

        Assert.True(await _sut.DeleteAsync(5, SuperAdmin));
        _repo.Verify(r => r.DeleteAsync(5), Times.Once);
    }
}
