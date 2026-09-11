using Librex.Application.DTOs.Products;
using Librex.Domain.Entities;
using Librex.Domain.Interfaces;

namespace Librex.Application.UseCases.Products;

public sealed class ProductService(IProductRepository repository) : IProductService
{
    public async Task<IEnumerable<ProductDto>> GetAllAsync(CancellationToken ct = default)
        => (await repository.GetAllAsync(ct)).Select(MapToDto);

    public async Task<ProductDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var product = await repository.GetByIdAsync(id, ct);
        return product is null ? null : MapToDto(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken ct = default)
    {
        var product = new Product { Name = dto.Name, Isbn = dto.Isbn, SchoolLevel = dto.SchoolLevel, UnitType = dto.UnitType, SupplierId = dto.SupplierId };
        return MapToDto(await repository.AddAsync(product, ct));
    }

    public async Task<ProductDto?> UpdateAsync(int id, UpdateProductDto dto, CancellationToken ct = default)
    {
        var product = await repository.GetByIdAsync(id, ct);
        if (product is null) return null;

        product.Name = dto.Name;
        product.Isbn = dto.Isbn;
        product.SchoolLevel = dto.SchoolLevel;
        product.UnitType = dto.UnitType;
        product.SupplierId = dto.SupplierId;

        await repository.UpdateAsync(product, ct);
        return MapToDto(product);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var product = await repository.GetByIdAsync(id, ct);
        if (product is null) return false;
        await repository.DeleteAsync(id, ct);
        return true;
    }

    private static ProductDto MapToDto(Product p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Isbn = p.Isbn,
        SchoolLevel = p.SchoolLevel,
        UnitType = p.UnitType,
        SupplierId = p.SupplierId,
        SupplierName = p.Supplier?.Name ?? string.Empty,
        IsActive = p.IsActive,
    };
}
