using Librex.Application.DTOs.Products;
using Librex.Application.UseCases.Products;
using Librex.Domain.Entities;
using Librex.Domain.Interfaces;
using Moq;

namespace Librex.Tests.Products;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _repo = new();
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
        _sut = new ProductService(_repo.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllActiveProducts()
    {
        _repo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Product>
            {
                new() { Id = 1, Name = "Book A", IsActive = true },
                new() { Id = 2, Name = "Book B", IsActive = true },
            });

        var result = (await _sut.GetAllAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("Book A", result[0].Name);
        Assert.Equal("Book B", result[1].Name);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsProduct()
    {
        _repo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Product { Id = 1, Name = "Book A", IsActive = true });

        var result = await _sut.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Book A", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        _repo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Product?)null);

        var result = await _sut.GetByIdAsync(99);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ValidDto_CreatesAndReturnsProduct()
    {
        var dto = new CreateProductDto { Name = "New Book" };

        _repo.Setup(r => r.AddAsync(It.IsAny<Product>()))
            .ReturnsAsync((Product p) => { p.Id = 42; return p; });

        var result = await _sut.CreateAsync(dto);

        Assert.Equal(42, result.Id);
        Assert.Equal("New Book", result.Name);
        _repo.Verify(r => r.AddAsync(It.Is<Product>(p => p.Name == "New Book")), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ExistingProduct_UpdatesAndReturnsDto()
    {
        var existing = new Product { Id = 1, Name = "Old Name", IsActive = true };
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        _repo.Setup(r => r.UpdateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);

        var result = await _sut.UpdateAsync(1, new UpdateProductDto { Name = "Updated Name" });

        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.Name);
        _repo.Verify(r => r.UpdateAsync(It.Is<Product>(p => p.Name == "Updated Name")), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingProduct_ReturnsNull()
    {
        _repo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Product?)null);

        var result = await _sut.UpdateAsync(99, new UpdateProductDto { Name = "X" });

        Assert.Null(result);
        _repo.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ExistingProduct_ReturnsTrueAndCallsDelete()
    {
        _repo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Product { Id = 1, Name = "Book", IsActive = true });
        _repo.Setup(r => r.DeleteAsync(1)).Returns(Task.CompletedTask);

        var result = await _sut.DeleteAsync(1);

        Assert.True(result);
        _repo.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingProduct_ReturnsFalse()
    {
        _repo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Product?)null);

        var result = await _sut.DeleteAsync(99);

        Assert.False(result);
        _repo.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }
}
