using Librex.Domain.Entities;

namespace Librex.Domain.Interfaces;

public interface ILoginAttemptRepository
{
    Task AddAsync(LoginAttempt attempt);
}
