using Librex.Domain.Entities;
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
            .Include(p => p.Publisher)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IEnumerable<Product>> GetAllAsync()
        => await _context.Products
            .Include(p => p.Publisher)
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

    public async Task DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is not null)
        {
            product.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }
}
