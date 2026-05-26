namespace Librex.Domain.Entities;

public class RemissionDetail : BaseEntity
{
    public int RemissionId { get; set; }
    public Remission Remission { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public string? City { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
