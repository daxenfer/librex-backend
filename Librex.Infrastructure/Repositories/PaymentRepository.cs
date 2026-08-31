using Librex.Domain.Entities;
using Librex.Domain.Enums;
using Librex.Domain.Interfaces;
using Librex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Librex.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly LibrexDbContext _context;

    public PaymentRepository(LibrexDbContext context)
    {
        _context = context;
    }

    public async Task<Payment?> GetByIdAsync(int id)
        => await _context.Payments.FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

    // Las asignaciones se filtran por IsActive: al eliminar una remisión, sus asignaciones se dan
    // de baja pero el pago sobrevive. Sin este filtro, AppliedAmount las seguiría contando y el
    // dinero nunca volvería a aparecer como anticipo del cliente.
    public async Task<Payment?> GetByIdWithCustomerAsync(int id)
        => await _context.Payments
            .Include(p => p.Customer)
            .Include(p => p.Allocations.Where(a => a.IsActive)).ThenInclude(a => a.Remission)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

    public async Task<IEnumerable<Payment>> GetAllAsync()
        => await _context.Payments
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.Date)
            .ToListAsync();

    public async Task<IEnumerable<Payment>> GetAllWithCustomerAsync()
        => await _context.Payments
            .Include(p => p.Customer)
            .Include(p => p.Allocations.Where(a => a.IsActive)).ThenInclude(a => a.Remission)
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.Date)
            .ToListAsync();

    // No filtra IsActive a propósito: el folio de un documento eliminado queda quemado y no se
    // reutiliza, evitando colisiones con el índice único de FolioNumber.
    public async Task<int> GetNextFolioAsync()
    {
        var max = await _context.Payments
            .Select(p => (int?)p.FolioNumber)
            .MaxAsync();
        return (max ?? 0) + 1;
    }

    public async Task<Payment> AddAsync(Payment payment)
    {
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();
        return payment;
    }

    public async Task UpdateAsync(Payment payment)
    {
        _context.Payments.Update(payment);
        await _context.SaveChangesAsync();
    }

    // Borrado lógico en cascada: la raíz y sus dependientes se marcan como inactivos en un
    // solo SaveChangesAsync. Nada se destruye, así que los documentos ya emitidos que citan
    // este registro conservan su historia intacta.
    public async Task DeleteAsync(int id)
    {
        var payment = await _context.Payments.FindAsync(id);
        if (payment is null) return;

        var dependents = await DeletionGraph.ResolveAsync(_context, DeletableEntity.Payment, id);
        dependents.Deactivate();
        payment.IsActive = false;
        await _context.SaveChangesAsync();
    }
}
