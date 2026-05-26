using Librex.Application.DTOs.Auth;
using Librex.Application.UseCases.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Librex.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _service;

    public AuthController(IAuthService service)
    {
        _service = service;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto dto)
    {
        var result = await _service.LoginAsync(dto);
        return result is null ? Unauthorized(new { message = "Invalid credentials" }) : Ok(result);
    }
}
