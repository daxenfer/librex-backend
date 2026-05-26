using System.ComponentModel.DataAnnotations;

namespace Librex.Application.DTOs.Payments;

public class CreatePaymentDto
{
    [Required]
    public int CustomerId { get; set; }

    [Required]
    public int RemissionId { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(50)]
    public string PaymentMethod { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Reference { get; set; }

    public string? Notes { get; set; }
}
