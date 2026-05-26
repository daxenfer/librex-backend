namespace Librex.Domain.Entities;

public class Payment : BaseEntity
{
    public int FolioNumber { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public int RemissionId { get; set; }
    public Remission Remission { get; set; } = null!;
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public string? Notes { get; set; }
}
