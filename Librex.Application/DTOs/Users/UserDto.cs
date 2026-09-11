namespace Librex.Application.DTOs.Users;

// Nunca expone PasswordHash: el hash no sale de la capa de datos por ningún endpoint.
public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
