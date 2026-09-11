namespace Librex.Domain.Constants;

// Política de bloqueo por cuenta. Complementa al rate limiting por IP: el límite por IP no frena
// un ataque distribuido contra una sola cuenta, y el bloqueo por cuenta no frena a quien barre
// muchas cuentas desde una IP. Juntos cubren los dos ejes.
public static class LockoutPolicy
{
    public const int MaxFailedAttempts = 10;

    // Temporal a propósito. Con bloqueo permanente, cualquiera que sepa que existe el usuario
    // "superadmin" podría dejar al dueño fuera de su propio sistema con diez intentos malos.
    public static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
}
