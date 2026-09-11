using System.ComponentModel.DataAnnotations;
using Librex.Application.Validation;

namespace Librex.Application.DTOs.Users;

public class CreateUserDto
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

    [Required]
    [StrongPassword]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;
}
