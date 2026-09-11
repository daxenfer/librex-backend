using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Librex.Application.DTOs.Auth;
using Librex.Domain.Constants;
using Librex.Domain.Entities;
using Librex.Domain.Enums;
using Librex.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Librex.Application.UseCases.Auth;

public sealed class AuthService(IUserRepository userRepository,
        ILoginAttemptRepository attemptRepository,
        IConfiguration configuration,
        TimeProvider clock) : IAuthService
{
    // Hash de una contraseña que nadie tiene. Se verifica contra él cuando el usuario no existe,
    // para que la respuesta tarde lo mismo que un intento contra una cuenta real: si solo se
    // ejecutara BCrypt en el caso "el usuario existe", el tiempo de respuesta delataría qué
    // nombres están dados de alta, que es la mitad del trabajo de quien ataca.
    private const string DummyHash = "$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy";

    // Devuelve null en todos los casos de fallo, sin distinguirlos: al usuario se le responde
    // siempre lo mismo. Decirle "cuenta bloqueada" o "ese usuario no existe" le confirmaría a
    // quien ataca qué cuentas son reales. El motivo verdadero queda en login_attempts.
    public async Task<LoginResponseDto?> LoginAsync(LoginDto dto, LoginRequestContext context)
    {
        var username = dto.Username?.Trim() ?? string.Empty;
        var user = await userRepository.GetByUsernameAsync(username);

        // Siempre se verifica, exista o no el usuario — ver DummyHash.
        var passwordMatches = BCrypt.Net.BCrypt.Verify(dto.Password, user?.PasswordHash ?? DummyHash);

        if (user is null)
            return await RejectAsync(username, LoginOutcome.UnknownUser, context);

        if (!user.IsActive)
            return await RejectAsync(username, LoginOutcome.InactiveUser, context);

        // El bloqueo se revisa antes que la contraseña: mientras dura, ni la correcta entra. Eso
        // es lo que obliga al ataque a esperar en vez de seguir probando.
        if (user.LockedOutUntil is { } until && until > clock.GetUtcNow().UtcDateTime)
            return await RejectAsync(username, LoginOutcome.LockedOut, context);

        if (!passwordMatches)
        {
            user.FailedLoginAttempts++;

            if (user.FailedLoginAttempts >= LockoutPolicy.MaxFailedAttempts)
            {
                user.LockedOutUntil = clock.GetUtcNow().UtcDateTime.Add(LockoutPolicy.LockoutDuration);
                user.FailedLoginAttempts = 0;   // el bloqueo sustituye al contador
            }

            await userRepository.UpdateAsync(user);
            return await RejectAsync(username, LoginOutcome.BadPassword, context);
        }

        // Entró: se limpia lo que haya quedado de intentos anteriores.
        user.FailedLoginAttempts = 0;
        user.LockedOutUntil = null;

        // Usuarios creados antes de que existiera el sello: se les asigna uno aquí, para que la
        // revocación funcione desde su primer acceso.
        if (string.IsNullOrEmpty(user.SecurityStamp))
            user.SecurityStamp = Guid.NewGuid().ToString("N");

        await userRepository.UpdateAsync(user);
        await LogAsync(username, LoginOutcome.Success, context);

        return BuildToken(user);
    }

    private async Task<LoginResponseDto?> RejectAsync(string username, LoginOutcome outcome, LoginRequestContext context)
    {
        await LogAsync(username, outcome, context);
        return null;
    }

    // La bitácora nunca debe tumbar el login: si la escritura falla, el usuario legítimo entra
    // igual. Se prefiere perder un renglón de auditoría a dejar a alguien fuera del sistema.
    private async Task LogAsync(string username, LoginOutcome outcome, LoginRequestContext context)
    {
        try
        {
            await attemptRepository.AddAsync(new LoginAttempt
            {
                Username = username.Length > 100 ? username[..100] : username,
                Succeeded = outcome == LoginOutcome.Success,
                Outcome = outcome,
                IpAddress = context.IpAddress,
                UserAgent = context.UserAgent,
            });
        }
        catch
        {
            // Sin reintento y sin propagar: el siguiente intento volverá a registrarse.
        }
    }

    private LoginResponseDto BuildToken(User user)
    {
        var secretKey = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key is not configured");
        var issuer = configuration["Jwt:Issuer"] ?? "LibrexAPI";
        var audience = configuration["Jwt:Audience"] ?? "LibrexClients";
        var expirationMinutes = int.Parse(configuration["Jwt:ExpirationMinutes"] ?? "480");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = clock.GetUtcNow().UtcDateTime.AddMinutes(expirationMinutes);

        // Los permisos se resuelven aquí, una sola vez, y viajan firmados dentro del token. Las
        // policies de Program.cs los leen de estos claims; el frontend recibe la misma lista en
        // la respuesta. Cambiar la matriz no afecta a los tokens ya emitidos: aplican al
        // siguiente login o cuando expire el actual.
        var permissions = Permissions.ForRole(user.Role);

        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role),
            new("fullName", user.FullName),
            new(SecurityClaims.Stamp, user.SecurityStamp),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        ];
        claims.AddRange(permissions.Select(p => new Claim(Permissions.ClaimType, p)));

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new LoginResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Username = user.Username,
            FullName = user.FullName,
            Role = user.Role,
            Permissions = permissions,
            ExpiresAt = expiresAt,
        };
    }
}
