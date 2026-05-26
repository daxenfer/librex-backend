namespace Librex.Domain.Entities;

public class ReturnNoteDetail : BaseEntity
{
    public int ReturnNoteId { get; set; }
    public ReturnNote ReturnNote { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
