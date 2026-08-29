using System.ComponentModel.DataAnnotations;

namespace Librex.Application.DTOs.Payments;

public class CreatePaymentDto : IValidatableObject
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

    // Lo distribuido en remisiones no puede exceder el monto recibido; el remanente queda como
    // anticipo. Es una regla de campos cruzados, así que va aquí y no en un [Range] individual.
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        const decimal epsilon = 0.01m;
        var assigned = Allocations?.Sum(a => a.Amount) ?? 0m;
        if (assigned > Amount + epsilon)
            yield return new ValidationResult(
                "Lo aplicado a remisiones no puede superar el monto del pago.",
                [nameof(Allocations)]);
    }
}
