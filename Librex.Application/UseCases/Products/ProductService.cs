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
        var product = new Product { Name = dto.Name, PublisherId = dto.PublisherId };
        return MapToDto(await _repository.AddAsync(product));
    }

    public async Task<ProductDto?> UpdateAsync(int id, UpdateProductDto dto)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product is null) return null;

        product.Name = dto.Name;
        product.PublisherId = dto.PublisherId;
        product.IsActive = dto.IsActive;

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
        PublisherId = p.PublisherId,
        PublisherName = p.Publisher?.Name ?? string.Empty,
        IsActive = p.IsActive,
    };
}
