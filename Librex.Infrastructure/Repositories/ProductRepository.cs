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

    public override async Task<Product?> GetByIdAsync(int id, CancellationToken ct = default)
        => await Set
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive, ct);

    public override async Task<IEnumerable<Product>> GetAllAsync(CancellationToken ct = default)
        => await Set
            .Include(p => p.Supplier)
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync(ct);

    // Se relee para que el producto recién creado salga con su Supplier cargado; quien llama
    // mapea a DTO de inmediato y sin esto el nombre del proveedor iría vacío.
    public override async Task<Product> AddAsync(Product product, CancellationToken ct = default)
    {
        await base.AddAsync(product);
        return await GetByIdAsync(product.Id) ?? product;
    }
}
