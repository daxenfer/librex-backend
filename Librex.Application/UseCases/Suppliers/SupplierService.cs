using Librex.Application.DTOs.Suppliers;
using Librex.Domain.Entities;
using Librex.Domain.Interfaces;

namespace Librex.Application.UseCases.Suppliers;

public sealed class SupplierService(ISupplierRepository repository) : ISupplierService
{
    public async Task<IEnumerable<SupplierDto>> GetAllAsync(CancellationToken ct = default)
        => (await repository.GetAllAsync(ct)).Select(MapToDto);

    public async Task<SupplierDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var supplier = await repository.GetByIdAsync(id, ct);
        return supplier is null ? null : MapToDto(supplier);
    }

    public async Task<SupplierDto> CreateAsync(CreateSupplierDto dto, CancellationToken ct = default)
    {
        var supplier = new Supplier
        {
            Name = dto.Name,
            Contact = dto.Contact,
            Phone = dto.Phone,
            Email = dto.Email,
        };
        return MapToDto(await repository.AddAsync(supplier, ct));
    }

    public async Task<SupplierDto?> UpdateAsync(int id, UpdateSupplierDto dto, CancellationToken ct = default)
    {
        var supplier = await repository.GetByIdAsync(id, ct);
        if (supplier is null) return null;

        supplier.Name = dto.Name;
        supplier.Contact = dto.Contact;
        supplier.Phone = dto.Phone;
        supplier.Email = dto.Email;

        await repository.UpdateAsync(supplier, ct);
        return MapToDto(supplier);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var supplier = await repository.GetByIdAsync(id, ct);
        if (supplier is null) return false;
        await repository.DeleteAsync(id, ct);
        return true;
    }

    private static SupplierDto MapToDto(Supplier p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Contact = p.Contact,
        Phone = p.Phone,
        Email = p.Email,
        IsActive = p.IsActive,
    };
}
