using Librex.Application.DTOs.Products;
using Librex.Domain.Entities;
using Librex.Domain.Interfaces;

namespace Librex.Application.UseCases.Products;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync()
        => (await _repository.GetAllAsync()).Select(MapToDto);

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await _repository.GetByIdAsync(id);
        return product is null ? null : MapToDto(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        var product = new Product { Name = dto.Name, Isbn = dto.Isbn, SchoolLevel = dto.SchoolLevel, UnitType = dto.UnitType, SupplierId = dto.SupplierId };
        return MapToDto(await _repository.AddAsync(product));
    }

    public async Task<ProductDto?> UpdateAsync(int id, UpdateProductDto dto)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product is null) return null;

        product.Name = dto.Name;
        product.Isbn = dto.Isbn;
        product.SchoolLevel = dto.SchoolLevel;
        product.UnitType = dto.UnitType;
        product.SupplierId = dto.SupplierId;

        await _repository.UpdateAsync(product);
        return MapToDto(product);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product is null) return false;
        await _repository.DeleteAsync(id);
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
