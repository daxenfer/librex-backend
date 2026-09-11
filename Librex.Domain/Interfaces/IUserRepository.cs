using Librex.Domain.Entities;

namespace Librex.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);

    // Mira también los inactivos: el índice único de `users.Username` no distingue, así que un
    // usuario dado de baja sigue ocupando su nombre.
    Task<bool> UsernameExistsAsync(string username, int? excludeId = null, CancellationToken ct = default);

    Task<int> CountActiveByRoleAsync(string role, CancellationToken ct = default);
}
