namespace Librex.Domain.Constants;

// Claims propios que Librex mete en el JWT, además de los estándar.
public static class SecurityClaims
{
    // Sello de seguridad del usuario al momento de emitir el token. Se compara contra el de la
    // base en cada petición: si no coincide, el token es de una versión anterior del usuario
    // (lo dieron de baja, le cambiaron el rol o la contraseña) y deja de valer.
    public const string Stamp = "stamp";
}
