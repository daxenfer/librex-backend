using System.ComponentModel.DataAnnotations;
using Librex.Application.Validation;

namespace Librex.Application.DTOs.Users;

public class ChangePasswordDto
{
    [Required]
    [StrongPassword]
    [MaxLength(100)]
    public string NewPassword { get; set; } = string.Empty;
}
