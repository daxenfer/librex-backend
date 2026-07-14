using System.ComponentModel.DataAnnotations;

namespace Librex.Application.DTOs.Payments;

public class PaymentAllocationDto
{
    public int RemissionId { get; set; }
    public string RemissionFolioFormatted { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class CreatePaymentAllocationDto
{
    [Required]
    public int RemissionId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }
}
