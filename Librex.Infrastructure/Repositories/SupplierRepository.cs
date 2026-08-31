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
        => await _context.Suppliers.FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

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

    // Borrado lógico en cascada: la raíz y sus dependientes se marcan como inactivos en un
    // solo SaveChangesAsync. Nada se destruye, así que los documentos ya emitidos que citan
    // este registro conservan su historia intacta.
    public async Task DeleteAsync(int id)
    {
        var supplier = await _context.Suppliers.FindAsync(id);
        if (supplier is null) return;

        var dependents = await DeletionGraph.ResolveAsync(_context, DeletableEntity.Supplier, id);
        dependents.Deactivate();
        supplier.IsActive = false;
        await _context.SaveChangesAsync();
    }
}
