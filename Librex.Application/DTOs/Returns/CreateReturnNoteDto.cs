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

public class CreateReturnNoteDto
{
    [Required]
    public int CustomerId { get; set; }

    [Required]
    public int RemissionId { get; set; }

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
}
