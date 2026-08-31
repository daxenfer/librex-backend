using Librex.Application.UseCases.Payments;
using Librex.Application.UseCases.Remissions;
using Librex.Infrastructure.Data;
using Librex.Infrastructure.Repositories;
using Librex.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Librex.Tests.Deletion;

// El borrado es lógico y en cascada: al eliminar una entidad, ella y sus dependientes quedan con
// IsActive = false, pero ninguna fila se destruye. La regla que fijan estas pruebas es que nunca
// se toca un renglón cuyo encabezado sobrevive: eliminar un producto o un proveedor no puede
// alterar los documentos ya emitidos que los citan.
public class CascadeDeleteTests : IDisposable
{
    private readonly LibrexDbContext _context = TestDbContextFactory.Create();

    public void Dispose() => _context.Dispose();

    // Conteo crudo de filas por tabla, sin filtrar por IsActive. Es lo que demuestra que el
    // borrado lógico no destruye nada.
    private async Task<int[]> RowCountsAsync() =>
    [
        await _context.Suppliers.CountAsync(),
        await _context.Products.CountAsync(),
        await _context.Customers.CountAsync(),
        await _context.Remissions.CountAsync(),
        await _context.RemissionDetails.CountAsync(),
        await _context.ReturnNotes.CountAsync(),
        await _context.ReturnNoteDetails.CountAsync(),
        await _context.Payments.CountAsync(),
        await _context.PaymentAllocations.CountAsync(),
    ];

    [Fact]
    public async Task DeleteCustomer_DeactivatesCustomerAndAllItsDocuments()
    {
        var data = await DeletionScenario.SeedAsync(_context);

        await new CustomerRepository(_context).DeleteAsync(data.Customer1.Id);

        Assert.False((await _context.Customers.FindAsync(data.Customer1.Id))!.IsActive);
        Assert.Empty(await _context.Remissions.Where(r => r.CustomerId == data.Customer1.Id && r.IsActive).ToListAsync());
        Assert.Empty(await _context.ReturnNotes.Where(r => r.CustomerId == data.Customer1.Id && r.IsActive).ToListAsync());
        Assert.Empty(await _context.Payments.Where(p => p.CustomerId == data.Customer1.Id && p.IsActive).ToListAsync());
    }

    [Fact]
    public async Task DeleteCustomer_DestroysNoRows()
    {
        var data = await DeletionScenario.SeedAsync(_context);
        var before = await RowCountsAsync();

        await new CustomerRepository(_context).DeleteAsync(data.Customer1.Id);

        Assert.Equal(before, await RowCountsAsync());
    }

    [Fact]
    public async Task DeleteCustomer_LeavesNoActiveOrphanLinesOrAllocations()
    {
        var data = await DeletionScenario.SeedAsync(_context);

        await new CustomerRepository(_context).DeleteAsync(data.Customer1.Id);

        // Solo siguen activas las del cliente 2: 1 línea de remisión, 1 de devolución, 1 asignación.
        Assert.Single(await _context.RemissionDetails.Where(d => d.IsActive).ToListAsync());
        Assert.Single(await _context.ReturnNoteDetails.Where(d => d.IsActive).ToListAsync());
        Assert.Single(await _context.PaymentAllocations.Where(a => a.IsActive).ToListAsync());
    }

    [Fact]
    public async Task DeleteCustomer_DoesNotTouchOtherCustomersData()
    {
        var data = await DeletionScenario.SeedAsync(_context);

        await new CustomerRepository(_context).DeleteAsync(data.Customer1.Id);

        Assert.True((await _context.Customers.FindAsync(data.Customer2.Id))!.IsActive);
        Assert.True((await _context.Remissions.FindAsync(data.Remission3.Id))!.IsActive);
        Assert.True((await _context.ReturnNotes.FindAsync(data.ReturnNote2.Id))!.IsActive);
        Assert.True((await _context.Payments.FindAsync(data.Payment2.Id))!.IsActive);
        Assert.Equal(2, await _context.Products.CountAsync(p => p.IsActive));
        Assert.True((await _context.Suppliers.FindAsync(data.Supplier.Id))!.IsActive);
    }

    [Fact]
    public async Task DeleteSupplier_DeactivatesItsProductsButKeepsDocumentLinesIntact()
    {
        var data = await DeletionScenario.SeedAsync(_context);

        await new SupplierRepository(_context).DeleteAsync(data.Supplier.Id);

        Assert.False((await _context.Suppliers.FindAsync(data.Supplier.Id))!.IsActive);
        Assert.Empty(await _context.Products.Where(p => p.IsActive).ToListAsync());

        // Lo que motivó el cambio: los documentos ya emitidos conservan todos sus renglones.
        Assert.Equal(4, await _context.RemissionDetails.CountAsync(d => d.IsActive));
        Assert.Equal(2, await _context.ReturnNoteDetails.CountAsync(d => d.IsActive));
        Assert.Equal(3, await _context.Remissions.CountAsync(r => r.IsActive));
        Assert.Equal(2, await _context.Payments.CountAsync(p => p.IsActive));
    }

    [Fact]
    public async Task DeleteProduct_KeepsAllDocumentLines()
    {
        var data = await DeletionScenario.SeedAsync(_context);

        await new ProductRepository(_context).DeleteAsync(data.Product1.Id);

        Assert.False((await _context.Products.FindAsync(data.Product1.Id))!.IsActive);
        Assert.True((await _context.Products.FindAsync(data.Product2.Id))!.IsActive);
        // Ni un solo renglón se dio de baja: pertenecen a documentos que sobreviven.
        Assert.Equal(4, await _context.RemissionDetails.CountAsync(d => d.IsActive));
        Assert.Equal(2, await _context.ReturnNoteDetails.CountAsync(d => d.IsActive));
    }

    [Fact]
    public async Task DeleteProduct_DoesNotChangeRemissionTotalOrProductName()
    {
        var data = await DeletionScenario.SeedAsync(_context);
        var service = new RemissionService(new RemissionRepository(_context));
        var before = (await service.GetByIdAsync(data.Remission1.Id))!;

        await new ProductRepository(_context).DeleteAsync(data.Product1.Id);

        var after = (await service.GetByIdAsync(data.Remission1.Id))!;
        Assert.Equal(before.Total, after.Total);
        Assert.Equal(before.Details.Count, after.Details.Count);
        // El nombre del producto eliminado se sigue resolviendo: el Include no filtra IsActive,
        // que es lo que mantiene íntegros los PDFs ya entregados.
        Assert.All(after.Details, d => Assert.False(string.IsNullOrEmpty(d.ProductName)));
    }

    [Fact]
    public async Task DeleteRemission_DeactivatesLinesAllocationsAndLinkedReturnNotes()
    {
        var data = await DeletionScenario.SeedAsync(_context);

        await new RemissionRepository(_context).DeleteAsync(data.Remission1.Id);

        Assert.False((await _context.Remissions.FindAsync(data.Remission1.Id))!.IsActive);
        Assert.Empty(await _context.RemissionDetails.Where(d => d.RemissionId == data.Remission1.Id && d.IsActive).ToListAsync());
        Assert.Empty(await _context.PaymentAllocations.Where(a => a.RemissionId == data.Remission1.Id && a.IsActive).ToListAsync());
        Assert.False((await _context.ReturnNotes.FindAsync(data.ReturnNote1.Id))!.IsActive);
    }

    [Fact]
    public async Task DeleteRemission_KeepsPaymentAndReturnsItsMoneyToUnapplied()
    {
        var data = await DeletionScenario.SeedAsync(_context);

        await new RemissionRepository(_context).DeleteAsync(data.Remission1.Id);

        // El pago de 500 tenía 200 aplicados a R1 y 100 a R2. Al eliminar R1 solo quedan 100
        // aplicados, y los otros 400 vuelven a ser anticipo a favor del cliente.
        var payment = (await _context.Payments.FindAsync(data.Payment1.Id))!;
        Assert.True(payment.IsActive);
        var applied = await _context.PaymentAllocations
            .Where(a => a.PaymentId == data.Payment1.Id && a.IsActive)
            .SumAsync(a => a.Amount);
        Assert.Equal(100m, applied);
        Assert.Equal(400m, payment.Amount - applied);
    }

    [Fact]
    public async Task DeleteRemission_PaymentDtoReturnsTheMoneyAsUnapplied()
    {
        var data = await DeletionScenario.SeedAsync(_context);
        var payments = new PaymentService(new PaymentRepository(_context), new RemissionRepository(_context));

        await new RemissionRepository(_context).DeleteAsync(data.Remission1.Id);
        // En producción el GET llega en otra petición con su propio DbContext. Sin limpiar el
        // tracker, la asignación ya cargada se re-adjuntaría a la navegación por fixup de EF,
        // ocultando si el Include filtra bien o no.
        _context.ChangeTracker.Clear();

        // Lo que ve cuentas por cobrar: el pago conserva sus 500, con solo 100 aplicados.
        var dto = (await payments.GetByIdAsync(data.Payment1.Id))!;
        Assert.Single(dto.Allocations);
        Assert.Equal(100m, dto.AppliedAmount);
        Assert.Equal(400m, dto.UnappliedAmount);
    }

    [Fact]
    public async Task DeleteReturnNote_DeactivatesOnlyItsLines()
    {
        var data = await DeletionScenario.SeedAsync(_context);

        await new ReturnNoteRepository(_context).DeleteAsync(data.ReturnNote1.Id);

        Assert.False((await _context.ReturnNotes.FindAsync(data.ReturnNote1.Id))!.IsActive);
        Assert.Single(await _context.ReturnNoteDetails.Where(d => d.IsActive).ToListAsync());
        Assert.True((await _context.Remissions.FindAsync(data.Remission1.Id))!.IsActive);
    }

    [Fact]
    public async Task DeletePayment_DeactivatesItsAllocationsButKeepsRemissions()
    {
        var data = await DeletionScenario.SeedAsync(_context);

        await new PaymentRepository(_context).DeleteAsync(data.Payment1.Id);

        Assert.False((await _context.Payments.FindAsync(data.Payment1.Id))!.IsActive);
        Assert.Empty(await _context.PaymentAllocations.Where(a => a.PaymentId == data.Payment1.Id && a.IsActive).ToListAsync());
        Assert.True((await _context.Remissions.FindAsync(data.Remission1.Id))!.IsActive);
        Assert.True((await _context.Remissions.FindAsync(data.Remission2.Id))!.IsActive);
    }

    [Fact]
    public async Task GetByIdAsync_DeletedRecord_ReturnsNull()
    {
        var data = await DeletionScenario.SeedAsync(_context);

        await new ProductRepository(_context).DeleteAsync(data.Product1.Id);
        await new CustomerRepository(_context).DeleteAsync(data.Customer2.Id);

        Assert.Null(await new ProductRepository(_context).GetByIdAsync(data.Product1.Id));
        Assert.Null(await new CustomerRepository(_context).GetByIdAsync(data.Customer2.Id));
        Assert.Null(await new RemissionRepository(_context).GetByIdAsync(data.Remission3.Id));
        Assert.Null(await new RemissionRepository(_context).GetByIdWithDetailsAsync(data.Remission3.Id));
        Assert.Null(await new ReturnNoteRepository(_context).GetByIdAsync(data.ReturnNote2.Id));
        Assert.Null(await new PaymentRepository(_context).GetByIdAsync(data.Payment2.Id));
    }

    [Fact]
    public async Task GetNextFolioAsync_AfterDeletingLastRemission_DoesNotReuseFolio()
    {
        var data = await DeletionScenario.SeedAsync(_context);
        var repository = new RemissionRepository(_context);

        await repository.DeleteAsync(data.Remission3.Id);

        // El folio 3 queda quemado: reutilizarlo chocaría con el índice único de FolioNumber.
        Assert.Equal(4, await repository.GetNextFolioAsync());
    }

    [Fact]
    public async Task DeleteCustomer_NonExistingId_DoesNothing()
    {
        await DeletionScenario.SeedAsync(_context);

        await new CustomerRepository(_context).DeleteAsync(9999);

        Assert.Equal(2, await _context.Customers.CountAsync(c => c.IsActive));
        Assert.Equal(3, await _context.Remissions.CountAsync(r => r.IsActive));
    }

    [Fact]
    public async Task DeleteCustomer_Twice_IsIdempotent()
    {
        var data = await DeletionScenario.SeedAsync(_context);
        var repository = new CustomerRepository(_context);

        await repository.DeleteAsync(data.Customer1.Id);
        var afterFirst = await RowCountsAsync();
        await repository.DeleteAsync(data.Customer1.Id);

        Assert.Equal(afterFirst, await RowCountsAsync());
        Assert.Single(await _context.Customers.Where(c => c.IsActive).ToListAsync());
    }
}
