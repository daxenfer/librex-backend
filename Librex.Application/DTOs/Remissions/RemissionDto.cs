namespace Librex.Application.DTOs.Remissions;

public class RemissionDto
{
    public int Id { get; set; }
    public int FolioNumber { get; set; }
    public string FolioFormatted { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerAddress { get; set; } = string.Empty;
    public string CustomerPostalCode { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerCity { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? SalesPerson { get; set; }
    public string? Notes { get; set; }
    public string? RecipientName { get; set; }
    public string? PurchaseOrder { get; set; }
    public DateTime DeliveryDate { get; set; }
    public DateTime PaymentDueDate { get; set; }
    public decimal ReturnPercentage { get; set; }
    public DateTime ReturnDueDate { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Total { get; set; }
    public bool IsActive { get; set; }
    public List<RemissionDetailDto> Details { get; set; } = [];
}
