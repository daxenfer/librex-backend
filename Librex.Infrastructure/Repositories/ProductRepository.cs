using Librex.Domain.Entities;
using Librex.Domain.Enums;
using Librex.Domain.Interfaces;
using Librex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Librex.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly LibrexDbContext _context;

    public ProductRepository(LibrexDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(int id)
        => await _context.Products
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

    public async Task<IEnumerable<Product>> GetAllAsync()
        => await _context.Products
            .Include(p => p.Supplier)
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();

    public async Task<Product> AddAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return await GetByIdAsync(product.Id) ?? product;
    }

    public async Task UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
    }

    // Borrado lógico en cascada: la raíz y sus dependientes se marcan como inactivos en un
    // solo SaveChangesAsync. Nada se destruye, así que los documentos ya emitidos que citan
    // este registro conservan su historia intacta.
    public async Task DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is null) return;

        var dependents = await DeletionGraph.ResolveAsync(_context, DeletableEntity.Product, id);
        dependents.Deactivate();
        product.IsActive = false;
        await _context.SaveChangesAsync();
    }
}
