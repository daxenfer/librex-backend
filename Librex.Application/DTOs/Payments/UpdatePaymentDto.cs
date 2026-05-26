namespace Librex.Application.DTOs.Payments;

public class UpdatePaymentDto : CreatePaymentDto
{
    public bool IsActive { get; set; } = true;
}
