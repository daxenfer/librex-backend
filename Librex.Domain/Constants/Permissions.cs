namespace Librex.Domain.Constants;

// Fuente única de la matriz de autorización. De aquí salen tres cosas y ninguna otra las define:
//   1. las policies que registra Program.cs (una por permiso),
//   2. los claims `perm` que AuthService mete en el JWT al hacer login,
//   3. la lista que el frontend recibe en LoginResponseDto.Permissions.
//
// El nombre del permiso ES el nombre de la policy: no hay tabla de traducción en medio.
//
// Leer no lleva permiso — cualquier usuario autenticado consulta. La matriz solo describe lo que
// escribe, borra o configura, que es lo que mantiene esta lista corta.
public static class Permissions
{
    public const string ClaimType = "perm";

    public const string SuppliersWrite = "suppliers.write";
    public const string SuppliersDelete = "suppliers.delete";
    public const string ProductsWrite = "products.write";
    public const string ProductsDelete = "products.delete";
    public const string CustomersWrite = "customers.write";
    public const string CustomersDelete = "customers.delete";
    public const string RemissionsWrite = "remissions.write";
    public const string RemissionsDelete = "remissions.delete";
    public const string ReturnsWrite = "returns.write";
    public const string ReturnsDelete = "returns.delete";
    public const string PaymentsWrite = "payments.write";
    public const string PaymentsDelete = "payments.delete";
    public const string SettingsManage = "settings.manage";
    public const string UsersManage = "users.manage";

    // El rol operativo: captura y corrige, pero nada irreversible ni global.
    private static readonly string[] UserPermissions =
    [
        SuppliersWrite,
        ProductsWrite,
        CustomersWrite,
        RemissionsWrite,
        ReturnsWrite,
        PaymentsWrite,
    ];

    // El dueño del negocio: además borra (el borrado es en cascada) y edita los datos de empresa
    // que salen impresos en todos los PDFs.
    private static readonly string[] AdministratorPermissions =
    [
        .. UserPermissions,
        SuppliersDelete,
        ProductsDelete,
        CustomersDelete,
        RemissionsDelete,
        ReturnsDelete,
        PaymentsDelete,
        SettingsManage,
    ];

    private static readonly string[] SuperAdminPermissions =
    [
        .. AdministratorPermissions,
        UsersManage,
    ];

    public static readonly IReadOnlyDictionary<string, string[]> ByRole =
        new Dictionary<string, string[]>
        {
            [Roles.User] = UserPermissions,
            [Roles.Administrator] = AdministratorPermissions,
            [Roles.SuperAdmin] = SuperAdminPermissions,
        };

    public static readonly string[] All = SuperAdminPermissions;

    // Un rol desconocido (dato viejo en la BD, typo en un seed manual) se queda sin permisos.
    // Nunca hereda los de administrador: fallar cerrado es la única opción segura aquí.
    public static string[] ForRole(string role)
        => ByRole.TryGetValue(role, out var permissions) ? permissions : [];
}
