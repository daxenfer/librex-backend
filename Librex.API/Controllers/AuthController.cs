using Librex.API.Security;
using Librex.Application.DTOs.Auth;
using Librex.Application.UseCases.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Librex.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService service) : ControllerBase
{
    private const int MaxUserAgentChars = 512;



    [HttpPost("login")]
    [EnableRateLimiting(RateLimitPolicies.Login)]
    [ProducesResponseType(typeof(LoginResponseDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(429)]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto dto, CancellationToken ct)
    {
        var result = await service.LoginAsync(dto, BuildContext(), ct);
        return result is null ? Unauthorized(new { message = "Invalid credentials" }) : Ok(result);
    }

    // La IP y el user agent solo alimentan la bitácora; no participan en decidir si alguien entra.
    // El user agent se trunca porque lo escribe quien llama y podría mandar cualquier cosa.
    private LoginRequestContext BuildContext()
    {
        var userAgent = Request.Headers.UserAgent.ToString();
        if (userAgent.Length > MaxUserAgentChars) userAgent = userAgent[..MaxUserAgentChars];

        return new LoginRequestContext(
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            string.IsNullOrWhiteSpace(userAgent) ? null : userAgent);
    }
}
