namespace Librex.Application.DTOs.ReturnNotes;

public class ReturnNoteDto
{
    public int Id { get; set; }
    public int FolioNumber { get; set; }
    public string FolioFormatted { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int? RemissionId { get; set; }
    public string RemissionFolioFormatted { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? Notes { get; set; }
    public string? ReceivedBy { get; set; }
    public decimal Discount { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Total { get; set; }
    public bool IsActive { get; set; }
    public List<ReturnNoteDetailDto> Details { get; set; } = [];
}
