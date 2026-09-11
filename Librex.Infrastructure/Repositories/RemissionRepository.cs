using Librex.Domain.Entities;
using Librex.Domain.Enums;
using Librex.Domain.Interfaces;
using Librex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Librex.Infrastructure.Repositories;

public sealed class RemissionRepository(LibrexDbContext context)
    : DocumentRepository<Remission>(context), IRemissionRepository
{
    protected override DeletableEntity? DeletionRoot => DeletableEntity.Remission;

    // Details no se filtra por IsActive a propósito: un renglón solo se da de baja cuando su
    // remisión también, y esta ya viene filtrada. Al editar, los renglones se reemplazan
    // físicamente, así que nunca hay filas inactivas colgando de una remisión activa.
    public async Task<Remission?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default)
        => await Set
            .Include(r => r.Customer)
            .Include(r => r.Details)
                .ThenInclude(d => d.Product)
                    .ThenInclude(p => p.Supplier)
            .FirstOrDefaultAsync(r => r.Id == id && r.IsActive, ct);

    public async Task<IEnumerable<Remission>> GetAllWithCustomerAsync(CancellationToken ct = default)
        => await Set
            .Include(r => r.Customer)
            .Include(r => r.Details)
            .Where(r => r.IsActive)
            .OrderByDescending(r => r.Date)
            .ToListAsync(ct);
}
