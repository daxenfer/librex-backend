namespace Librex.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;

    // Cambia cada vez que el usuario deja de ser quien era para efectos de autorización: baja,
    // cambio de rol o cambio de contraseña. El token lo lleva como claim y se compara en cada
    // petición, así que un token viejo deja de servir en cuanto el sello cambia. Sin esto, dar de
    // baja a alguien no lo saca: su JWT sigue siendo válido hasta que expire.
    public string SecurityStamp { get; set; } = Guid.NewGuid().ToString("N");

    // Contador de intentos fallidos consecutivos. Un login exitoso lo regresa a cero.
    public int FailedLoginAttempts { get; set; }

    // Hasta cuándo está bloqueado el acceso. Null o en el pasado significa que puede entrar.
    public DateTime? LockedOutUntil { get; set; }
}
