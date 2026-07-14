namespace Librex.Application.DTOs.Suppliers;

public class UpdateSupplierDto : CreateSupplierDto
{
    public bool IsActive { get; set; } = true;
}
