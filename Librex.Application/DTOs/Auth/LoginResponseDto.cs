namespace Librex.Application.DTOs.Auth;

public sealed record LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;

    // Lo que el usuario puede hacer, ya resuelto desde su rol. El frontend no conoce la matriz:
    // solo recibe esta lista y pregunta si contiene el permiso que le interesa.
    public string[] Permissions { get; set; } = [];

    public DateTime ExpiresAt { get; set; }
}
