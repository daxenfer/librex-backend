using Librex.Domain.Entities;
using Librex.Domain.Enums;
using Librex.Domain.Interfaces;
using Librex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Librex.Infrastructure.Repositories;

public sealed class SupplierRepository(LibrexDbContext context)
    : Repository<Supplier>(context), ISupplierRepository
{
    protected override DeletableEntity? DeletionRoot => DeletableEntity.Supplier;

    public override async Task<IEnumerable<Supplier>> GetAllAsync()
        => await Set.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync();
}
