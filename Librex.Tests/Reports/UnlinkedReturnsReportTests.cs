using Librex.Domain.Entities;
using Librex.Infrastructure.Data;
using Librex.Infrastructure.Repositories;
using Librex.Tests.Helpers;

namespace Librex.Tests.Reports;

// Una devolución sin remisión no se le atribuye a ningún proveedor: se reporta aparte, igual que
// los anticipos. Estas pruebas fijan esa asimetría y su único límite — el reporte de cantidades,
// que es de movimiento físico, la sigue contando.
public class UnlinkedReturnsReportTests : IDisposable
{
    private readonly LibrexDbContext _context = TestDbContextFactory.Create();
    private readonly ReportRepository _sut;

    public UnlinkedReturnsReportTests()
    {
        _sut = new ReportRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    // El escenario compartido solo trae devoluciones ligadas, así que la suelta se siembra aquí.
    // Folio 3: el escenario ya usó el 1 y el 2.
    private async Task SeedUnlinkedReturnAsync(Customer customer, Product product, string reason)
    {
        var note = EntityBuilder.NewReturnNote(customer, null, 3,
            EntityBuilder.NewReturnNoteDetail(product, quantity: 2m, unitPrice: 100m));
        note.UnlinkedReason = reason;
        _context.Add(note);
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetUnlinkedReturnsAsync_OnlyIncludesNotesWithoutRemission()
    {
        var data = await DeletionScenario.SeedAsync(_context);
        await SeedUnlinkedReturnAsync(data.Customer1, data.Product1, "Material de muestra");

        var report = await _sut.GetUnlinkedReturnsAsync();

        // Las dos devoluciones del escenario están ligadas: solo debe salir la que sembramos.
        var row = Assert.Single(report.Rows);
        Assert.Equal(data.Customer1.Id, row.CustomerId);
        Assert.Equal("Escuela Central", row.CustomerName);
        Assert.Equal(1, row.NoteCount);
        Assert.Equal(200m, row.UnlinkedAmount);
        Assert.Contains("Material de muestra", row.ReasonSummary);
        Assert.Equal(200m, report.TotalUnlinked);
    }

    [Fact]
    public async Task GetUnlinkedReturnsAsync_WithOnlyLinkedNotes_ReturnsEmpty()
    {
        await DeletionScenario.SeedAsync(_context);

        var report = await _sut.GetUnlinkedReturnsAsync();

        Assert.Empty(report.Rows);
        Assert.Equal(0m, report.TotalUnlinked);
    }

    [Fact]
    public async Task GetBySupplierAsync_WithSupplier_ExcludesUnlinkedReturns()
    {
        var data = await DeletionScenario.SeedAsync(_context);
        var before = await _sut.GetBySupplierAsync(data.Supplier.Id);

        await SeedUnlinkedReturnAsync(data.Customer1, data.Product1, "Material de muestra");

        // El saldo del proveedor no se mueve: esa devolución no corresponde a ninguna venta suya.
        var after = await _sut.GetBySupplierAsync(data.Supplier.Id);
        Assert.Equal(before.Totals.TotalReturns, after.Totals.TotalReturns);
        Assert.Equal(before.Totals.Balance, after.Totals.Balance);
    }

    [Fact]
    public async Task GetBySupplierAsync_WithoutSupplier_StillCountsUnlinkedReturns()
    {
        var data = await DeletionScenario.SeedAsync(_context);
        var before = await _sut.GetBySupplierAsync(null);

        await SeedUnlinkedReturnAsync(data.Customer1, data.Product1, "Material de muestra");

        // A nivel cliente sí cuenta: el cliente devolvió mercancía, con o sin remisión.
        var after = await _sut.GetBySupplierAsync(null);
        Assert.Equal(before.Totals.TotalReturns + 200m, after.Totals.TotalReturns);
    }

    [Fact]
    public async Task GetSalesByProductAsync_StillCountsUnlinkedReturns()
    {
        var data = await DeletionScenario.SeedAsync(_context);
        var before = await _sut.GetSalesByProductAsync(data.Supplier.Id);

        await SeedUnlinkedReturnAsync(data.Customer1, data.Product1, "Material de muestra");

        // Este reporte es de movimiento físico: los libros volvieron y deben verse.
        var after = await _sut.GetSalesByProductAsync(data.Supplier.Id);
        Assert.Equal(before.GrandTotalReturned + 2, after.GrandTotalReturned);
    }
}
