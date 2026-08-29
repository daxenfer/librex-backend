using Librex.Infrastructure.Data;
using Librex.Infrastructure.Repositories;
using Librex.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Librex.Tests.Deletion;

// El borrado ahora es físico y en cascada: al borrar una entidad desaparecen ella y todos sus
// dependientes, sin dejar huérfanos y sin tocar los documentos de otros clientes.
public class CascadeDeleteTests : IDisposable
{
    private readonly LibrexDbContext _context = TestDbContextFactory.Create();

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task DeleteCustomer_RemovesCustomerAndAllItsDocuments()
    {
        var data = await DeletionScenario.SeedAsync(_context);

        await new CustomerRepository(_context).DeleteAsync(data.Customer1.Id);

        Assert.Null(await _context.Customers.FindAsync(data.Customer1.Id));
        Assert.Empty(await _context.Remissions.Where(r => r.CustomerId == data.Customer1.Id).ToListAsync());
        Assert.Empty(await _context.ReturnNotes.Where(r => r.CustomerId == data.Customer1.Id).ToListAsync());
        Assert.Empty(await _context.Payments.Where(p => p.CustomerId == data.Customer1.Id).ToListAsync());
    }

    [Fact]
    public async Task DeleteCustomer_LeavesNoOrphanLinesOrAllocations()
    {
        var data = await DeletionScenario.SeedAsync(_context);

        await new CustomerRepository(_context).DeleteAsync(data.Customer1.Id);

        // Solo deben sobrevivir las del cliente 2: 1 línea de remisión, 1 de devolución, 1 asignación.
        Assert.Single(await _context.RemissionDetails.ToListAsync());
        Assert.Single(await _context.ReturnNoteDetails.ToListAsync());
        Assert.Single(await _context.PaymentAllocations.ToListAsync());
    }

    [Fact]
    public async Task DeleteCustomer_DoesNotTouchOtherCustomersData()
    {
        var data = await DeletionScenario.SeedAsync(_context);

        await new CustomerRepository(_context).DeleteAsync(data.Customer1.Id);

        Assert.NotNull(await _context.Customers.FindAsync(data.Customer2.Id));
        Assert.NotNull(await _context.Remissions.FindAsync(data.Remission3.Id));
        Assert.NotNull(await _context.ReturnNotes.FindAsync(data.ReturnNote2.Id));
        Assert.NotNull(await _context.Payments.FindAsync(data.Payment2.Id));
        Assert.Equal(2, await _context.Products.CountAsync());
        Assert.NotNull(await _context.Suppliers.FindAsync(data.Supplier.Id));
    }

    [Fact]
    public async Task DeleteSupplier_RemovesItsProductsAndTheirDocumentLines()
    {
        var data = await DeletionScenario.SeedAsync(_context);

        await new SupplierRepository(_context).DeleteAsync(data.Supplier.Id);

        Assert.Null(await _context.Suppliers.FindAsync(data.Supplier.Id));
        Assert.Empty(await _context.Products.ToListAsync());
        Assert.Empty(await _context.RemissionDetails.ToListAsync());
        Assert.Empty(await _context.ReturnNoteDetails.ToListAsync());
        // Los encabezados sobreviven: borrar un proveedor no borra remisiones ni pagos.
        Assert.Equal(3, await _context.Remissions.CountAsync());
        Assert.Equal(2, await _context.Payments.CountAsync());
    }

    [Fact]
    public async Task DeleteProduct_RemovesOnlyItsOwnDocumentLines()
    {
        var data = await DeletionScenario.SeedAsync(_context);

        await new ProductRepository(_context).DeleteAsync(data.Product1.Id);

        Assert.Null(await _context.Products.FindAsync(data.Product1.Id));
        Assert.NotNull(await _context.Products.FindAsync(data.Product2.Id));
        Assert.Empty(await _context.RemissionDetails.Where(d => d.ProductId == data.Product1.Id).ToListAsync());
        Assert.Equal(2, await _context.RemissionDetails.CountAsync());
        Assert.Single(await _context.ReturnNoteDetails.ToListAsync());
    }

    [Fact]
    public async Task DeleteRemission_RemovesLinesAllocationsAndLinkedReturnNotes()
    {
        var data = await DeletionScenario.SeedAsync(_context);

        await new RemissionRepository(_context).DeleteAsync(data.Remission1.Id);

        Assert.Null(await _context.Remissions.FindAsync(data.Remission1.Id));
        Assert.Empty(await _context.RemissionDetails.Where(d => d.RemissionId == data.Remission1.Id).ToListAsync());
        Assert.Empty(await _context.PaymentAllocations.Where(a => a.RemissionId == data.Remission1.Id).ToListAsync());
        Assert.Null(await _context.ReturnNotes.FindAsync(data.ReturnNote1.Id));
        // El pago en sí sobrevive; solo pierde la asignación a esa remisión.
        Assert.NotNull(await _context.Payments.FindAsync(data.Payment1.Id));
        Assert.Single(await _context.PaymentAllocations.Where(a => a.PaymentId == data.Payment1.Id).ToListAsync());
    }

    [Fact]
    public async Task DeleteReturnNote_RemovesOnlyItsLines()
    {
        var data = await DeletionScenario.SeedAsync(_context);

        await new ReturnNoteRepository(_context).DeleteAsync(data.ReturnNote1.Id);

        Assert.Null(await _context.ReturnNotes.FindAsync(data.ReturnNote1.Id));
        Assert.Single(await _context.ReturnNoteDetails.ToListAsync());
        Assert.NotNull(await _context.Remissions.FindAsync(data.Remission1.Id));
    }

    [Fact]
    public async Task DeletePayment_RemovesItsAllocationsButKeepsRemissions()
    {
        var data = await DeletionScenario.SeedAsync(_context);

        await new PaymentRepository(_context).DeleteAsync(data.Payment1.Id);

        Assert.Null(await _context.Payments.FindAsync(data.Payment1.Id));
        Assert.Empty(await _context.PaymentAllocations.Where(a => a.PaymentId == data.Payment1.Id).ToListAsync());
        Assert.NotNull(await _context.Remissions.FindAsync(data.Remission1.Id));
        Assert.NotNull(await _context.Remissions.FindAsync(data.Remission2.Id));
    }

    [Fact]
    public async Task DeleteCustomer_NonExistingId_DoesNothing()
    {
        var data = await DeletionScenario.SeedAsync(_context);
        var remissionsBefore = await _context.Remissions.CountAsync();

        await new CustomerRepository(_context).DeleteAsync(9999);

        Assert.Equal(2, await _context.Customers.CountAsync());
        Assert.Equal(remissionsBefore, await _context.Remissions.CountAsync());
    }

    [Fact]
    public async Task DeleteCustomer_Twice_SecondCallFindsNothingToDelete()
    {
        var data = await DeletionScenario.SeedAsync(_context);
        var repository = new CustomerRepository(_context);

        await repository.DeleteAsync(data.Customer1.Id);
        await repository.DeleteAsync(data.Customer1.Id);

        Assert.Null(await _context.Customers.FindAsync(data.Customer1.Id));
        Assert.Single(await _context.Customers.ToListAsync());
    }
}
