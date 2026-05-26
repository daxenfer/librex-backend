using Librex.Application.DTOs.Auth;

namespace Librex.Application.UseCases.Auth;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginDto dto);
}
