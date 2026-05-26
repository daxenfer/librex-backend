using System.ComponentModel.DataAnnotations;

namespace Librex.Application.DTOs.Customers;

public class CreateCustomerDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
}
