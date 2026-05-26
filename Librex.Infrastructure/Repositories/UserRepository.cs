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

    public async Task<User?> GetByUsernameAsync(string username)
        => await _context.Users.FirstOrDefaultAsync(u => u.Username == username && u.IsActive);
}
