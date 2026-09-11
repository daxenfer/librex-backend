using Librex.Application.DTOs.Users;
using Librex.Domain.Constants;
using Librex.Domain.Entities;
using Librex.Domain.Exceptions;
using Librex.Domain.Interfaces;

namespace Librex.Application.UseCases.Users;

// Alta, baja y cambios de usuarios. Todas las reglas de guarda viven aquí y no en el controlador:
// son de negocio, y el frontend solo las refleja deshabilitando botones.
public sealed class UserService(IUserRepository repository, TimeProvider clock) : IUserService
{
    public async Task<IEnumerable<UserDto>> GetAllAsync()
        => (await repository.GetAllAsync()).Select(MapToDto);

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var user = await repository.GetByIdAsync(id);
        return user is null || !user.IsActive ? null : MapToDto(user);
    }

    // Refleja la matriz compilada, sin tocar la base. Los roles salen en el orden de Roles.All, de
    // mayor a menor alcance, que es como se pintan las columnas en la pantalla de usuarios. Se
    // copian los arreglos para no entregar los estáticos de Domain, que son compartidos.
    public PermissionMatrixDto GetPermissionMatrix() => new()
    {
        Roles = [.. Roles.All],
        Permissions = [.. Permissions.All],
        Grants = Roles.All.ToDictionary(role => role, role => Permissions.ForRole(role).ToArray()),
    };

    public async Task<UserDto> CreateAsync(CreateUserDto dto, ActingUser actor)
    {
        var role = dto.Role.Trim();
        var username = dto.Username.Trim();

        EnsureRoleExists(role);
        EnsureCanAssign(role, actor);

        if (await repository.UsernameExistsAsync(username))
            throw new BusinessRuleException($"El usuario \"{username}\" ya existe.");

        var user = new User
        {
            Username = username,
            FullName = dto.FullName.Trim(),
            Role = role,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
        };

        return MapToDto(await repository.AddAsync(user));
    }

    public async Task<UserDto?> UpdateAsync(int id, UpdateUserDto dto, ActingUser actor)
    {
        var user = await repository.GetByIdAsync(id);
        if (user is null || !user.IsActive) return null;

        var role = dto.Role.Trim();
        var username = dto.Username.Trim();

        EnsureRoleExists(role);
        EnsureCanTouch(user, actor);
        EnsureCanAssign(role, actor);

        if (id == actor.Id && role != user.Role)
            throw new BusinessRuleException("No puedes cambiar tu propio rol.");

        // Degradar al último SuperAdmin dejaría el sistema sin nadie que administre usuarios, y
        // sin forma de arreglarlo desde la aplicación.
        if (user.Role == Roles.SuperAdmin && role != Roles.SuperAdmin)
            await EnsureNotLastSuperAdminAsync();

        if (await repository.UsernameExistsAsync(username, id))
            throw new BusinessRuleException($"El usuario \"{username}\" ya existe.");

        // El rol y el nombre de usuario viajan dentro del token. Si cambian, hay que renovar el
        // sello para que las sesiones abiertas dejen de valer: si no, a quien acaban de degradar
        // le seguiría funcionando su token de administrador hasta que expirara.
        var identityChanged = role != user.Role || username != user.Username;

        user.Username = username;
        user.FullName = dto.FullName.Trim();
        user.Role = role;
        user.ModifiedAt = clock.GetUtcNow().UtcDateTime;
        if (identityChanged) user.SecurityStamp = Guid.NewGuid().ToString("N");

        await repository.UpdateAsync(user);
        return MapToDto(user);
    }

    public async Task<bool> ChangePasswordAsync(int id, ChangePasswordDto dto, ActingUser actor)
    {
        var user = await repository.GetByIdAsync(id);
        if (user is null || !user.IsActive) return false;

        EnsureCanTouch(user, actor);

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.ModifiedAt = clock.GetUtcNow().UtcDateTime;

        // Cambiar la contraseña cierra las sesiones abiertas de esa cuenta. Es el caso que más
        // importa: si se restablece porque alguien la tenía, el token robado deja de servir.
        user.SecurityStamp = Guid.NewGuid().ToString("N");

        // Restablecer la contraseña también levanta el bloqueo: es la salida cuando alguien se
        // dejó fuera solo.
        user.FailedLoginAttempts = 0;
        user.LockedOutUntil = null;

        await repository.UpdateAsync(user);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, ActingUser actor)
    {
        var user = await repository.GetByIdAsync(id);
        if (user is null || !user.IsActive) return false;

        if (id == actor.Id)
            throw new BusinessRuleException("No puedes desactivar tu propio usuario.");

        EnsureCanTouch(user, actor);

        if (user.Role == Roles.SuperAdmin)
            await EnsureNotLastSuperAdminAsync();

        await repository.DeleteAsync(id);   // baja lógica: IsActive = false
        return true;
    }

    // ---- Reglas de guarda ----

    private static void EnsureRoleExists(string role)
    {
        if (!Roles.IsValid(role))
            throw new BusinessRuleException($"El rol \"{role}\" no existe.");
    }

    // Anti-escalada de privilegios: nadie otorga un rol con más alcance que el propio. Hoy queda
    // cerrado por construcción (solo SuperAdmin tiene users.manage), pero la regla va explícita
    // para que siga valiendo si mañana el permiso se le devuelve al cliente.
    private static void EnsureCanAssign(string role, ActingUser actor)
    {
        if (Roles.Rank(role) > Roles.Rank(actor.Role))
            throw new BusinessRuleException("No puedes asignar un rol con más alcance que el tuyo.");
    }

    // La otra mitad de lo mismo: tampoco se edita ni se da de baja a alguien de mayor rango.
    private static void EnsureCanTouch(User target, ActingUser actor)
    {
        if (Roles.Rank(target.Role) > Roles.Rank(actor.Role))
            throw new BusinessRuleException("No puedes modificar a un usuario con más alcance que el tuyo.");
    }

    private async Task EnsureNotLastSuperAdminAsync()
    {
        if (await repository.CountActiveByRoleAsync(Roles.SuperAdmin) <= 1)
            throw new BusinessRuleException("Debe quedar al menos un super administrador activo.");
    }

    private static UserDto MapToDto(User u) => new()
    {
        Id = u.Id,
        Username = u.Username,
        FullName = u.FullName,
        Role = u.Role,
        CreatedAt = u.CreatedAt,
    };
}
