namespace Librex.Application.DTOs.Remissions;

public class RemissionDetailDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Isbn { get; set; }
    public string? SupplierName { get; set; }
    public string? Teacher { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }
}
