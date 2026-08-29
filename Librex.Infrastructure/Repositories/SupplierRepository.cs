using Librex.Domain.Entities;
using Librex.Domain.Enums;
using Librex.Domain.Interfaces;
using Librex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Librex.Infrastructure.Repositories;

public class SupplierRepository : ISupplierRepository
{
    private readonly LibrexDbContext _context;

    public SupplierRepository(LibrexDbContext context)
    {
        _context = context;
    }

    public async Task<Supplier?> GetByIdAsync(int id)
        => await _context.Suppliers.FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IEnumerable<Supplier>> GetAllAsync()
        => await _context.Suppliers
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();

    public async Task<Supplier> AddAsync(Supplier supplier)
    {
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();
        return supplier;
    }

    public async Task UpdateAsync(Supplier supplier)
    {
        _context.Suppliers.Update(supplier);
        await _context.SaveChangesAsync();
    }

    // Borrado físico en cascada. Un solo SaveChangesAsync para que EF lo resuelva
    // dentro de una transacción: o se va todo, o no se va nada.
    public async Task DeleteAsync(int id)
    {
        var supplier = await _context.Suppliers.FindAsync(id);
        if (supplier is null) return;

        var dependents = await DeletionGraph.ResolveAsync(_context, DeletableEntity.Supplier, id);
        dependents.RemoveFrom(_context);
        _context.Suppliers.Remove(supplier);
        await _context.SaveChangesAsync();
    }
}
