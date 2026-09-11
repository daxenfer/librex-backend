using System.ComponentModel.DataAnnotations;

namespace Librex.Application.DTOs.Payments;

public sealed record PaymentAllocationDto
{
    public int RemissionId { get; set; }
    public string RemissionFolioFormatted { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public sealed record CreatePaymentAllocationDto
{
    [Required]
    public int RemissionId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }
}
