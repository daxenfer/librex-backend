namespace Librex.Domain.Constants;

// Los roles del sistema, de mayor a menor alcance.
//
// SuperAdmin es el rol del proveedor del sistema, no del cliente: es el único que administra
// usuarios. Así ningún usuario del cliente puede darse permisos a sí mismo ni crear cuentas.
public static class Roles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Administrator = "Administrator";
    public const string User = "User";

    public static readonly string[] All = [SuperAdmin, Administrator, User];

    public static bool IsValid(string role) => All.Contains(role);

    // Jerarquía para la regla anti-escalada de UserService: nadie asigna ni toca un rol de rango
    // mayor al propio. Un rol desconocido vale 0, así que nunca gana una comparación.
    public static int Rank(string role) => role switch
    {
        SuperAdmin => 3,
        Administrator => 2,
        User => 1,
        _ => 0,
    };
}
