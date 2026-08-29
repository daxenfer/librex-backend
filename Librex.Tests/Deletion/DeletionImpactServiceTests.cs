using Librex.Application.DTOs.Deletion;
using Librex.Application.UseCases.Deletion;
using Librex.Domain.Enums;
using Librex.Infrastructure.Data;
using Librex.Infrastructure.Repositories;
using Librex.Tests.Helpers;

namespace Librex.Tests.Deletion;

public class DeletionImpactServiceTests : IDisposable
{
    private readonly LibrexDbContext _context = TestDbContextFactory.Create();
    private readonly DeletionService _sut;

    public DeletionImpactServiceTests()
    {
        _sut = new DeletionService(new DeletionRepository(_context));
    }

    public void Dispose() => _context.Dispose();

    private static int CountOf(DeletionImpactDto impact, string entityName)
        => impact.Items.SingleOrDefault(i => i.EntityName == entityName)?.Count ?? 0;

    [Fact]
    public async Task GetImpactAsync_Customer_CountsEveryDependentDocument()
    {
        var data = await DeletionScenario.SeedAsync(_context);

        var impact = await _sut.GetImpactAsync(DeletableEntity.Customer, data.Customer1.Id);

        Assert.NotNull(impact);
        Assert.Equal("Escuela Central", impact.Label);
        Assert.Equal(2, CountOf(impact, "Remisiones"));
        Assert.Equal(3, CountOf(impact, "Líneas de remisión"));
        Assert.Equal(1, CountOf(impact, "Devoluciones"));
        Assert.Equal(1, CountOf(impact, "Líneas de devolución"));
        Assert.Equal(1, CountOf(impact, "Pagos"));
        Assert.Equal(2, CountOf(impact, "Aplicaciones de pago"));
        Assert.Equal(10, impact.TotalDependents);
    }

    [Fact]
    public async Task GetImpactAsync_Supplier_CountsProductsAndTheirDocumentLines()
    {
        var data = await DeletionScenario.SeedAsync(_context);

        var impact = await _sut.GetImpactAsync(DeletableEntity.Supplier, data.Supplier.Id);

        Assert.NotNull(impact);
        Assert.Equal(2, CountOf(impact, "Productos"));
        Assert.Equal(4, CountOf(impact, "Líneas de remisión"));
        Assert.Equal(2, CountOf(impact, "Líneas de devolución"));
        Assert.Equal(0, CountOf(impact, "Remisiones"));
    }

    [Fact]
    public async Task GetImpactAsync_Product_CountsOnlyItsOwnDocumentLines()
    {
        var data = await DeletionScenario.SeedAsync(_context);

        var impact = await _sut.GetImpactAsync(DeletableEntity.Product, data.Product1.Id);

        Assert.NotNull(impact);
        Assert.Equal("Matemáticas 1", impact.Label);
        Assert.Equal(2, CountOf(impact, "Líneas de remisión"));
        Assert.Equal(1, CountOf(impact, "Líneas de devolución"));
    }

    [Fact]
    public async Task GetImpactAsync_Remission_CountsLinesAllocationsAndLinkedReturnNotes()
    {
        var data = await DeletionScenario.SeedAsync(_context);

        var impact = await _sut.GetImpactAsync(DeletableEntity.Remission, data.Remission1.Id);

        Assert.NotNull(impact);
        Assert.Equal("Folio 1", impact.Label);
        Assert.Equal(2, CountOf(impact, "Líneas de remisión"));
        Assert.Equal(1, CountOf(impact, "Aplicaciones de pago"));
        Assert.Equal(1, CountOf(impact, "Devoluciones"));
        Assert.Equal(1, CountOf(impact, "Líneas de devolución"));
        Assert.Equal(0, CountOf(impact, "Pagos"));
    }

    [Fact]
    public async Task GetImpactAsync_ReturnNote_CountsOnlyItsLines()
    {
        var data = await DeletionScenario.SeedAsync(_context);

        var impact = await _sut.GetImpactAsync(DeletableEntity.ReturnNote, data.ReturnNote1.Id);

        Assert.NotNull(impact);
        Assert.Equal(1, CountOf(impact, "Líneas de devolución"));
        Assert.Equal(1, impact.TotalDependents);
    }

    [Fact]
    public async Task GetImpactAsync_Payment_CountsOnlyItsAllocations()
    {
        var data = await DeletionScenario.SeedAsync(_context);

        var impact = await _sut.GetImpactAsync(DeletableEntity.Payment, data.Payment1.Id);

        Assert.NotNull(impact);
        Assert.Equal(2, CountOf(impact, "Aplicaciones de pago"));
        Assert.Equal(2, impact.TotalDependents);
    }

    [Fact]
    public async Task GetImpactAsync_EntityWithoutDependents_ReturnsEmptyItems()
    {
        var customer = EntityBuilder.NewCustomer("Cliente Sin Movimientos");
        _context.Add(customer);
        await _context.SaveChangesAsync();

        var impact = await _sut.GetImpactAsync(DeletableEntity.Customer, customer.Id);

        Assert.NotNull(impact);
        Assert.Empty(impact.Items);
        Assert.Equal(0, impact.TotalDependents);
    }

    [Fact]
    public async Task GetImpactAsync_NonExistingId_ReturnsNull()
    {
        await DeletionScenario.SeedAsync(_context);

        var impact = await _sut.GetImpactAsync(DeletableEntity.Customer, 9999);

        Assert.Null(impact);
    }
}
