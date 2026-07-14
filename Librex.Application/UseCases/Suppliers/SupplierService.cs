using Librex.Application.DTOs.Suppliers;
using Librex.Domain.Entities;
using Librex.Domain.Interfaces;

namespace Librex.Application.UseCases.Suppliers;

public class SupplierService : ISupplierService
{
    private readonly ISupplierRepository _repository;

    public SupplierService(ISupplierRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<SupplierDto>> GetAllAsync()
        => (await _repository.GetAllAsync()).Select(MapToDto);

    public async Task<SupplierDto?> GetByIdAsync(int id)
    {
        var supplier = await _repository.GetByIdAsync(id);
        return supplier is null ? null : MapToDto(supplier);
    }

    public async Task<SupplierDto> CreateAsync(CreateSupplierDto dto)
    {
        var supplier = new Supplier
        {
            Name = dto.Name,
            Contact = dto.Contact,
            Phone = dto.Phone,
            Email = dto.Email,
        };
        return MapToDto(await _repository.AddAsync(supplier));
    }

    public async Task<SupplierDto?> UpdateAsync(int id, UpdateSupplierDto dto)
    {
        var supplier = await _repository.GetByIdAsync(id);
        if (supplier is null) return null;

        supplier.Name = dto.Name;
        supplier.Contact = dto.Contact;
        supplier.Phone = dto.Phone;
        supplier.Email = dto.Email;
        supplier.IsActive = dto.IsActive;

        await _repository.UpdateAsync(supplier);
        return MapToDto(supplier);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var supplier = await _repository.GetByIdAsync(id);
        if (supplier is null) return false;
        await _repository.DeleteAsync(id);
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
