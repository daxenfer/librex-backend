using Librex.Application.DTOs.Users;

namespace Librex.Application.UseCases.Users;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllAsync();
    Task<UserDto?> GetByIdAsync(int id);
    PermissionMatrixDto GetPermissionMatrix();
    Task<UserDto> CreateAsync(CreateUserDto dto, ActingUser actor);
    Task<UserDto?> UpdateAsync(int id, UpdateUserDto dto, ActingUser actor);
    Task<bool> ChangePasswordAsync(int id, ChangePasswordDto dto, ActingUser actor);
    Task<bool> DeleteAsync(int id, ActingUser actor);
}
