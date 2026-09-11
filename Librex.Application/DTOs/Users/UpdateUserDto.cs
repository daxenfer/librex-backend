using System.ComponentModel.DataAnnotations;

namespace Librex.Application.DTOs.Users;

// La contraseña no se toca aquí: se cambia por su propio endpoint.
public class UpdateUserDto
{
    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Role { get; set; } = string.Empty;
}
