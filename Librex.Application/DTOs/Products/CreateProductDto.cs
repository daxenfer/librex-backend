using System.ComponentModel.DataAnnotations;

namespace Librex.Application.DTOs.Products;

public class CreateProductDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public int PublisherId { get; set; }
}
