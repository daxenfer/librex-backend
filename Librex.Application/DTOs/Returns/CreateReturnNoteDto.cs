using System.ComponentModel.DataAnnotations;

namespace Librex.Application.DTOs.ReturnNotes;

public class CreateReturnNoteDetailDto
{
    [Required]
    public int ProductId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Quantity { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; }
}

public class CreateReturnNoteDto : IValidatableObject
{
    [Required]
    public int CustomerId { get; set; }

    // Opcional pero desalentado: si la devolución no se liga a una remisión hay que decir por qué.
    public int? RemissionId { get; set; }

    [MaxLength(500)]
    public string? UnlinkedReason { get; set; }

    [Required]
    public DateTime Date { get; set; }

    public string? Notes { get; set; }

    [MaxLength(200)]
    public string? ReceivedBy { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Discount { get; set; }

    [Required]
    [MinLength(1)]
    public List<CreateReturnNoteDetailDto> Details { get; set; } = [];

    // Capturar una devolución sin remisión sigue permitido, pero no en silencio. Es una regla de
    // campos cruzados, así que va aquí y no en un [Required] individual.
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (RemissionId is null && string.IsNullOrWhiteSpace(UnlinkedReason))
            yield return new ValidationResult(
                "Indica el motivo por el que la devolución no corresponde a una remisión.",
                [nameof(UnlinkedReason)]);
    }
}
