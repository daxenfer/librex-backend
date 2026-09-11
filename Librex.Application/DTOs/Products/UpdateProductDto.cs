namespace Librex.Application.DTOs.Products;

// Sin campos propios: IsActive no lo edita el usuario, solo lo mueve el borrado lógico.
public sealed record UpdateProductDto : CreateProductDto
{
}
