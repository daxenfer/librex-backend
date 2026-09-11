using Librex.Domain.Entities;
using Librex.Domain.Interfaces;
using Librex.Infrastructure.Data;

namespace Librex.Infrastructure.Repositories;

public class LoginAttemptRepository : ILoginAttemptRepository
{
    private readonly LibrexDbContext _context;

    public LoginAttemptRepository(LibrexDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(LoginAttempt attempt)
    {
        _context.LoginAttempts.Add(attempt);
        await _context.SaveChangesAsync();
    }
}
