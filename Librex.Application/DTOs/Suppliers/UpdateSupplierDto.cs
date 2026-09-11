namespace Librex.Application.DTOs.Suppliers;

// Sin campos propios: IsActive no lo edita el usuario, solo lo mueve el borrado lógico.
public sealed record UpdateSupplierDto : CreateSupplierDto
{
}
