namespace Librex.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Isbn { get; set; }
    public string? SchoolLevel { get; set; }
    public string UnitType { get; set; } = "Unidad";
    public int SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;
}
