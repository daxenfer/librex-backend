namespace Librex.Application.DTOs.Customers;

public class UpdateCustomerDto : CreateCustomerDto
{
    public bool IsActive { get; set; } = true;
}
