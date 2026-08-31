using Librex.Application.DTOs.Customers;
using Librex.Domain.Entities;
using Librex.Domain.Interfaces;

namespace Librex.Application.UseCases.Customers;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repository;

    public CustomerService(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<CustomerDto>> GetAllAsync()
        => (await _repository.GetAllAsync()).Select(MapToDto);

    public async Task<CustomerDto?> GetByIdAsync(int id)
    {
        var customer = await _repository.GetByIdAsync(id);
        return customer is null ? null : MapToDto(customer);
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto)
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
        return MapToDto(await _repository.AddAsync(customer));
    }

    public async Task<CustomerDto?> UpdateAsync(int id, UpdateCustomerDto dto)
    {
        var customer = await _repository.GetByIdAsync(id);
        if (customer is null) return null;

        customer.Name = dto.Name;
        customer.Contact = dto.Contact;
        customer.Address = dto.Address;
        customer.PostalCode = dto.PostalCode;
        customer.Phone = dto.Phone;
        customer.City = dto.City;

        await _repository.UpdateAsync(customer);
        return MapToDto(customer);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var customer = await _repository.GetByIdAsync(id);
        if (customer is null) return false;
        await _repository.DeleteAsync(id);
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
