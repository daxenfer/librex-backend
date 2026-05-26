using System.ComponentModel.DataAnnotations;

namespace Librex.Application.DTOs.Publishers;

public class CreatePublisherDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Contact { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(150)]
    public string? Email { get; set; }
}
