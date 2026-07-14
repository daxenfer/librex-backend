using System.ComponentModel.DataAnnotations;

namespace Librex.Application.DTOs.Payments;

public class CreatePaymentDto
{
    [Required]
    public int CustomerId { get; set; }

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

    [MaxLength(200)]
    public string? ReceivedFrom { get; set; }

    [MaxLength(500)]
    public string? Concept { get; set; }

    [MaxLength(200)]
    public string? CollectedBy { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    // Puede ir vacía: el pago se captura a nivel cliente (recibo) y se asigna a remisiones
    // después, en Cuentas por Cobrar. El remanente queda como anticipo a favor del cliente.
    public List<CreatePaymentAllocationDto> Allocations { get; set; } = [];
}
