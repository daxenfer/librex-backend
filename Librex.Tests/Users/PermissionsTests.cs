using Librex.Domain.Constants;

namespace Librex.Tests.Users;

// La matriz es la fuente única de la autorización: si se corre un permiso de rol, se cae aquí y
// no en producción.
public class PermissionsTests
{
    [Fact]
    public void User_CanWriteButNeverDelete()
    {
        var permissions = Permissions.ForRole(Roles.User);

        Assert.Contains(Permissions.RemissionsWrite, permissions);
        Assert.DoesNotContain(Permissions.RemissionsDelete, permissions);
        Assert.DoesNotContain(Permissions.SettingsManage, permissions);
    }

    [Fact]
    public void Administrator_DeletesAndConfiguresButDoesNotManageUsers()
    {
        var permissions = Permissions.ForRole(Roles.Administrator);

        Assert.Contains(Permissions.RemissionsDelete, permissions);
        Assert.Contains(Permissions.SettingsManage, permissions);
        Assert.DoesNotContain(Permissions.UsersManage, permissions);
    }

    [Fact]
    public void UsersManage_BelongsToSuperAdminOnly()
    {
        Assert.Contains(Permissions.UsersManage, Permissions.ForRole(Roles.SuperAdmin));

        foreach (var role in Roles.All.Where(r => r != Roles.SuperAdmin))
            Assert.DoesNotContain(Permissions.UsersManage, Permissions.ForRole(role));
    }

    // Fallar cerrado: un rol viejo o mal escrito en la BD no puede heredar nada.
    [Fact]
    public void UnknownRole_GetsNothing()
        => Assert.Empty(Permissions.ForRole("Editorial"));

    [Fact]
    public void EveryPermissionHasAPolicy_AllIsTheSuperAdminSet()
    {
        Assert.Equal(Permissions.ForRole(Roles.SuperAdmin), Permissions.All);
        Assert.Equal(Permissions.All.Distinct().Count(), Permissions.All.Length);
    }
}
