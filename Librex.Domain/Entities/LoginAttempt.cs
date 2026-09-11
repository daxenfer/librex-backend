namespace Librex.Domain.Entities;

using Librex.Domain.Enums;

// Registro inmutable de un intento de acceso, exitoso o no. No hereda de BaseEntity por la misma
// razón que ErrorLog: no es una entidad editable, es una escritura de bitácora.
//
// Existe porque un 401 no es una excepción y por lo tanto ErrorLoggingMiddleware no lo ve: sin
// esta tabla, un ataque de fuerza bruta no dejaría ningún rastro consultable.
public class LoginAttempt
{
    public int Id { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

    // Lo que se tecleó, no una FK: hay que poder registrar intentos contra usuarios que no existen.
    public string Username { get; set; } = string.Empty;

    public bool Succeeded { get; set; }
    public LoginOutcome Outcome { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
