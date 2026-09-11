using Librex.Domain.Entities;
using Librex.Domain.Enums;
using Librex.Domain.Interfaces;
using Librex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Librex.Infrastructure.Repositories;

public sealed class ReturnNoteRepository(LibrexDbContext context)
    : DocumentRepository<ReturnNote>(context), IReturnNoteRepository
{
    protected override DeletableEntity? DeletionRoot => DeletableEntity.ReturnNote;

    public async Task<ReturnNote?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default)
        => await Set
            .Include(r => r.Customer)
            .Include(r => r.Remission)
            .Include(r => r.Details)
                .ThenInclude(d => d.Product)
                    .ThenInclude(p => p.Supplier)
            .FirstOrDefaultAsync(r => r.Id == id && r.IsActive, ct);

    public async Task<IEnumerable<ReturnNote>> GetAllWithCustomerAsync(CancellationToken ct = default)
        => await Set
            .Include(r => r.Customer)
            .Include(r => r.Remission)
            .Include(r => r.Details)
                .ThenInclude(d => d.Product)
            .Where(r => r.IsActive)
            .OrderByDescending(r => r.Date)
            .ToListAsync(ct);
}
