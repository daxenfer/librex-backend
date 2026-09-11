using Librex.Domain.Entities;
using Librex.Domain.Interfaces;
using Librex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Librex.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly LibrexDbContext _context;

    public UserRepository(LibrexDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(int id)
        => await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

    public async Task<IEnumerable<User>> GetAllAsync()
        => await _context.Users.Where(u => u.IsActive).ToListAsync();

    public async Task<User> AddAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user is not null)
        {
            user.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }

    // A propósito NO filtra por IsActive: el login necesita distinguir "no existe" de "está
    // dado de baja" para poder registrarlo en la bitácora. Quien llama decide si lo deja pasar.
    public async Task<User?> GetByUsernameAsync(string username)
        => await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

    public async Task<bool> UsernameExistsAsync(string username, int? excludeId = null)
        => await _context.Users.AnyAsync(u => u.Username == username && (excludeId == null || u.Id != excludeId));

    public async Task<int> CountActiveByRoleAsync(string role)
        => await _context.Users.CountAsync(u => u.Role == role && u.IsActive);
}
