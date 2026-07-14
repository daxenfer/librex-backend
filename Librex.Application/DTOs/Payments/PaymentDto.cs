namespace Librex.Application.DTOs.Payments;

public class PaymentDto
{
    public int Id { get; set; }
    public int FolioNumber { get; set; }
    public string FolioFormatted { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public decimal AppliedAmount { get; set; }
    public decimal UnappliedAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public string? ReceivedFrom { get; set; }
    public string? Concept { get; set; }
    public string? CollectedBy { get; set; }
    public string? City { get; set; }
    public bool IsActive { get; set; }
    public List<PaymentAllocationDto> Allocations { get; set; } = [];
}
