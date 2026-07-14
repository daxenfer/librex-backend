using System.ComponentModel.DataAnnotations;

namespace Librex.Application.DTOs.Remissions;

public class CreateRemissionDetailDto
{
    [Required]
    public int ProductId { get; set; }

    [MaxLength(200)]
    public string? Teacher { get; set; }

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

    [MaxLength(200)]
    public string? SalesPerson { get; set; }

    public string? Notes { get; set; }

    [MaxLength(200)]
    public string? RecipientName { get; set; }

    [MaxLength(200)]
    public string? PurchaseOrder { get; set; }

    [Required]
    public DateTime DeliveryDate { get; set; }

    [Required]
    public DateTime PaymentDueDate { get; set; }

    [Required]
    [Range(0, 100)]
    public decimal ReturnPercentage { get; set; }

    [Required]
    public DateTime ReturnDueDate { get; set; }

    [Range(0, double.MaxValue)]
    public decimal DiscountAmount { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "La remisión debe tener al menos un producto.")]
    public List<CreateRemissionDetailDto> Details { get; set; } = [];
}
