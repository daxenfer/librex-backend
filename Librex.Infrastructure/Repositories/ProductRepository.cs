using Librex.Domain.Entities;
using Librex.Domain.Enums;
using Librex.Domain.Interfaces;
using Librex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Librex.Infrastructure.Repositories;

public sealed class ProductRepository(LibrexDbContext context)
    : Repository<Product>(context), IProductRepository
{
    protected override DeletableEntity? DeletionRoot => DeletableEntity.Product;

    public override async Task<Product?> GetByIdAsync(int id)
        => await Set
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

    public override async Task<IEnumerable<Product>> GetAllAsync()
        => await Set
            .Include(p => p.Supplier)
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();

    // Se relee para que el producto recién creado salga con su Supplier cargado; quien llama
    // mapea a DTO de inmediato y sin esto el nombre del proveedor iría vacío.
    public override async Task<Product> AddAsync(Product product)
    {
        await base.AddAsync(product);
        return await GetByIdAsync(product.Id) ?? product;
    }
}
