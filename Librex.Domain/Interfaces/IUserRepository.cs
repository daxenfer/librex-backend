using Librex.Domain.Entities;

namespace Librex.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username);

    // Mira también los inactivos: el índice único de `users.Username` no distingue, así que un
    // usuario dado de baja sigue ocupando su nombre.
    Task<bool> UsernameExistsAsync(string username, int? excludeId = null);

    Task<int> CountActiveByRoleAsync(string role);
}
