using System.ComponentModel.DataAnnotations;

namespace Librex.Application.DTOs.Auth;

public sealed record LoginDto
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
