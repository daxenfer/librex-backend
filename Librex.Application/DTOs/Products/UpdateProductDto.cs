namespace Librex.Application.DTOs.Products;

public class UpdateProductDto : CreateProductDto
{
    public bool IsActive { get; set; } = true;
}
