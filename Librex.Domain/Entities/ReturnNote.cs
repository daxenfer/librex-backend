namespace Librex.Domain.Entities;

public class ReturnNote : BaseEntity
{
    public int FolioNumber { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public int? RemissionId { get; set; }
    public Remission? Remission { get; set; }
    // Por qué esta devolución no corresponde a ninguna remisión. Obligatorio cuando RemissionId
    // es null: capturar una nota suelta sigue siendo posible, pero no en silencio.
    public string? UnlinkedReason { get; set; }
    public DateTime Date { get; set; }
    public string? Notes { get; set; }
    public string? ReceivedBy { get; set; }
    public decimal Discount { get; set; }
    public ICollection<ReturnNoteDetail> Details { get; set; } = [];
}
