using Librex.Application.DTOs.Users;

namespace Librex.Application.UseCases.Users;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllAsync(CancellationToken ct = default);
    Task<UserDto?> GetByIdAsync(int id, CancellationToken ct = default);
    PermissionMatrixDto GetPermissionMatrix();
    Task<UserDto> CreateAsync(CreateUserDto dto, ActingUser actor, CancellationToken ct = default);
    Task<UserDto?> UpdateAsync(int id, UpdateUserDto dto, ActingUser actor, CancellationToken ct = default);
    Task<bool> ChangePasswordAsync(int id, ChangePasswordDto dto, ActingUser actor, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, ActingUser actor, CancellationToken ct = default);
}
