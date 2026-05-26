using Librex.Domain.Entities;
using Librex.Domain.Interfaces;
using Librex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Librex.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly LibrexDbContext _context;

    public CustomerRepository(LibrexDbContext context)
    {
        _context = context;
    }

    public async Task<Customer?> GetByIdAsync(int id)
        => await _context.Customers.FirstOrDefaultAsync(c => c.Id == id);

    public async Task<IEnumerable<Customer>> GetAllAsync()
        => await _context.Customers
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();

    public async Task<Customer> AddAsync(Customer customer)
    {
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        return customer;
    }

    public async Task UpdateAsync(Customer customer)
    {
        _context.Customers.Update(customer);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer is not null)
        {
            customer.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }
}
