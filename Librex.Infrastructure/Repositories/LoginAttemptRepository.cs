using Librex.Domain.Entities;
using Librex.Domain.Interfaces;
using Librex.Infrastructure.Data;

namespace Librex.Infrastructure.Repositories;

public sealed class LoginAttemptRepository(LibrexDbContext context) : ILoginAttemptRepository
{
    public async Task AddAsync(LoginAttempt attempt, CancellationToken ct = default)
    {
        context.LoginAttempts.Add(attempt);
        await context.SaveChangesAsync(ct);
    }
}
