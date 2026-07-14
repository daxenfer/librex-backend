using System.ComponentModel.DataAnnotations;

namespace Librex.Application.DTOs.Products;

public class CreateProductDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Isbn { get; set; }

    [Required]
    public int SupplierId { get; set; }
}
