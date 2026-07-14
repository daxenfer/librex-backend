namespace Librex.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Isbn { get; set; }
    public int SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;
}
