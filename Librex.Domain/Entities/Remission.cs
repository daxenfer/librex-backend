namespace Librex.Domain.Entities;

public class Remission : BaseEntity
{
    public int FolioNumber { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public DateTime Date { get; set; }
    public string? SalesPerson { get; set; }
    public string? Notes { get; set; }
    public string? RecipientName { get; set; }
    public decimal Discount { get; set; }
    public ICollection<RemissionDetail> Details { get; set; } = [];
}
