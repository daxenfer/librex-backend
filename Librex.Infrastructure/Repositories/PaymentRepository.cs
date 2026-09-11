using Librex.Domain.Entities;
using Librex.Domain.Enums;
using Librex.Domain.Interfaces;
using Librex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Librex.Infrastructure.Repositories;

public sealed class PaymentRepository(LibrexDbContext context)
    : DocumentRepository<Payment>(context), IPaymentRepository
{
    protected override DeletableEntity? DeletionRoot => DeletableEntity.Payment;

    // Las asignaciones se filtran por IsActive: al eliminar una remisión, sus asignaciones se dan
    // de baja pero el pago sobrevive. Sin este filtro, AppliedAmount las seguiría contando y el
    // dinero nunca volvería a aparecer como anticipo del cliente.
    public async Task<Payment?> GetByIdWithCustomerAsync(int id, CancellationToken ct = default)
        => await Set
            .Include(p => p.Customer)
            .Include(p => p.Allocations.Where(a => a.IsActive)).ThenInclude(a => a.Remission)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive, ct);

    public async Task<IEnumerable<Payment>> GetAllWithCustomerAsync(CancellationToken ct = default)
        => await Set
            .Include(p => p.Customer)
            .Include(p => p.Allocations.Where(a => a.IsActive)).ThenInclude(a => a.Remission)
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.Date)
            .ToListAsync(ct);
}
