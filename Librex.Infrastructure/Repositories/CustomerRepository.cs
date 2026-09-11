using Librex.Domain.Entities;
using Librex.Domain.Enums;
using Librex.Domain.Interfaces;
using Librex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Librex.Infrastructure.Repositories;

public sealed class CustomerRepository(LibrexDbContext context)
    : Repository<Customer>(context), ICustomerRepository
{
    protected override DeletableEntity? DeletionRoot => DeletableEntity.Customer;

    public override async Task<IEnumerable<Customer>> GetAllAsync(CancellationToken ct = default)
        => await Set.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync(ct);
}
