namespace Librex.Application.DTOs.Users;

// La matriz de autorización tal como está compilada, para mostrarla como referencia. Es de solo
// lectura: los permisos de cada rol viven en Librex.Domain/Constants/Permissions.cs y cambiarlos
// requiere recompilar, no un dato de la base.
public sealed record PermissionMatrixDto
{
    // Roles de mayor a menor alcance, que es el orden en que se pintan las columnas.
    public string[] Roles { get; set; } = [];

    // Todos los permisos que existen. Leer no aparece aquí: no lleva permiso.
    public string[] Permissions { get; set; } = [];

    // Rol -> permisos que tiene.
    public Dictionary<string, string[]> Grants { get; set; } = [];
}
