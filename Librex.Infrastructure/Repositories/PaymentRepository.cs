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
        => await _context.Payments.FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Payment?> GetByIdWithCustomerAsync(int id)
        => await _context.Payments
            .Include(p => p.Customer)
            .Include(p => p.Allocations).ThenInclude(a => a.Remission)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IEnumerable<Payment>> GetAllAsync()
        => await _context.Payments
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.Date)
            .ToListAsync();

    public async Task<IEnumerable<Payment>> GetAllWithCustomerAsync()
        => await _context.Payments
            .Include(p => p.Customer)
            .Include(p => p.Allocations).ThenInclude(a => a.Remission)
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.Date)
            .ToListAsync();

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

    // Borrado físico en cascada. Un solo SaveChangesAsync para que EF lo resuelva
    // dentro de una transacción: o se va todo, o no se va nada.
    public async Task DeleteAsync(int id)
    {
        var payment = await _context.Payments.FindAsync(id);
        if (payment is null) return;

        var dependents = await DeletionGraph.ResolveAsync(_context, DeletableEntity.Payment, id);
        dependents.RemoveFrom(_context);
        _context.Payments.Remove(payment);
        await _context.SaveChangesAsync();
    }
}
