using Librex.Application.DTOs.Customers;
using Librex.Domain.Entities;
using Librex.Domain.Interfaces;

namespace Librex.Application.UseCases.Customers;

public sealed class CustomerService(ICustomerRepository repository) : ICustomerService
{
    public async Task<IEnumerable<CustomerDto>> GetAllAsync(CancellationToken ct = default)
        => (await repository.GetAllAsync(ct)).Select(MapToDto);

    public async Task<CustomerDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var customer = await repository.GetByIdAsync(id, ct);
        return customer is null ? null : MapToDto(customer);
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto, CancellationToken ct = default)
    {
        var customer = new Customer
        {
            Name = dto.Name,
            Contact = dto.Contact,
            Address = dto.Address,
            PostalCode = dto.PostalCode,
            Phone = dto.Phone,
            City = dto.City,
        };
        return MapToDto(await repository.AddAsync(customer, ct));
    }

    public async Task<CustomerDto?> UpdateAsync(int id, UpdateCustomerDto dto, CancellationToken ct = default)
    {
        var customer = await repository.GetByIdAsync(id, ct);
        if (customer is null) return null;

        customer.Name = dto.Name;
        customer.Contact = dto.Contact;
        customer.Address = dto.Address;
        customer.PostalCode = dto.PostalCode;
        customer.Phone = dto.Phone;
        customer.City = dto.City;

        await repository.UpdateAsync(customer, ct);
        return MapToDto(customer);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var customer = await repository.GetByIdAsync(id, ct);
        if (customer is null) return false;
        await repository.DeleteAsync(id, ct);
        return true;
    }

    private static CustomerDto MapToDto(Customer c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Contact = c.Contact,
        Address = c.Address,
        PostalCode = c.PostalCode,
        Phone = c.Phone,
        City = c.City,
        IsActive = c.IsActive,
    };
}
