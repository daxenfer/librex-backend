using Librex.Domain.Entities;
using Librex.Domain.Enums;
using Librex.Domain.Interfaces;
using Librex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Librex.Infrastructure.Repositories;

// Un usuario no arrastra dependientes: DeletionRoot es null, así que DeleteAsync solo lo marca
// inactivo. Sus remisiones y pagos no le pertenecen.
public sealed class UserRepository(LibrexDbContext context)
    : Repository<User>(context), IUserRepository
{
    protected override DeletableEntity? DeletionRoot => null;

    // A propósito NO filtra por IsActive: el login necesita distinguir "no existe" de "está
    // dado de baja" para poder registrarlo en la bitácora, y la validación del token necesita
    // encontrar al usuario desactivado para poder rechazarlo. Quien llama decide si lo deja pasar.
    public override async Task<User?> GetByIdAsync(int id)
        => await Set.FirstOrDefaultAsync(u => u.Id == id);

    public async Task<User?> GetByUsernameAsync(string username)
        => await Set.FirstOrDefaultAsync(u => u.Username == username);

    public async Task<bool> UsernameExistsAsync(string username, int? excludeId = null)
        => await Set.AnyAsync(u => u.Username == username && (excludeId == null || u.Id != excludeId));

    public async Task<int> CountActiveByRoleAsync(string role)
        => await Set.CountAsync(u => u.Role == role && u.IsActive);
}
