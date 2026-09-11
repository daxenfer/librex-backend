namespace Librex.Application.UseCases.Auth;

// De dónde vino el intento de acceso. Lo arma el controlador leyendo la petición y solo sirve
// para la bitácora: ni la IP ni el user agent participan en decidir si alguien entra.
public record LoginRequestContext(string? IpAddress, string? UserAgent);
