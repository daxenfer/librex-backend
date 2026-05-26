using System.ComponentModel.DataAnnotations;

namespace Librex.Application.DTOs.Remissions;

public class CreateRemissionDetailDto
{
    [Required]
    public int ProductId { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Quantity { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; }
}

public class CreateRemissionDto
{
    [Required]
    public int CustomerId { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [MaxLength(200)]
    public string? SalesPerson { get; set; }

    public string? Notes { get; set; }

    [MaxLength(200)]
    public string? RecipientName { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Discount { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "La remisión debe tener al menos un producto.")]
    public List<CreateRemissionDetailDto> Details { get; set; } = [];
}
