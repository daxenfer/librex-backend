using Librex.Domain.Entities;
using Librex.Domain.Enums;
using Librex.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Librex.Infrastructure.Data;

// Registros dependientes que caen junto con una entidad raíz.
// El orden en RemoveFrom no es arbitrario: borra hijos antes que padres para no violar
// las FKs Restrict de Postgres.
internal sealed class DeletionSet
{
    public List<PaymentAllocation> PaymentAllocations { get; init; } = [];
    public List<Payment> Payments { get; init; } = [];
    public List<ReturnNoteDetail> ReturnNoteDetails { get; init; } = [];
    public List<ReturnNote> ReturnNotes { get; init; } = [];
    public List<RemissionDetail> RemissionDetails { get; init; } = [];
    public List<Remission> Remissions { get; init; } = [];
    public List<Product> Products { get; init; } = [];

    public IReadOnlyList<DeletionDependent> ToDependents() =>
    [
        .. new DeletionDependent[]
        {
            new(DependentKind.Product, Products.Count),
            new(DependentKind.Remission, Remissions.Count),
            new(DependentKind.RemissionDetail, RemissionDetails.Count),
            new(DependentKind.ReturnNote, ReturnNotes.Count),
            new(DependentKind.ReturnNoteDetail, ReturnNoteDetails.Count),
            new(DependentKind.Payment, Payments.Count),
            new(DependentKind.PaymentAllocation, PaymentAllocations.Count),
        }.Where(d => d.Count > 0)
    ];

    public void RemoveFrom(LibrexDbContext context)
    {
        context.PaymentAllocations.RemoveRange(PaymentAllocations);
        context.Payments.RemoveRange(Payments);
        context.ReturnNoteDetails.RemoveRange(ReturnNoteDetails);
        context.ReturnNotes.RemoveRange(ReturnNotes);
        context.RemissionDetails.RemoveRange(RemissionDetails);
        context.Remissions.RemoveRange(Remissions);
        context.Products.RemoveRange(Products);
    }
}

// Única definición del grafo de dependencias del borrado en cascada. La usan tanto el conteo de
// impacto (DeletionRepository) como el borrado real (los DeleteAsync de cada repositorio), para
// que lo que se anuncia y lo que se borra no puedan divergir.
internal static class DeletionGraph
{
    public static Task<DeletionSet> ResolveAsync(LibrexDbContext context, DeletableEntity entity, int id) => entity switch
    {
        DeletableEntity.Customer => ResolveCustomerAsync(context, id),
        DeletableEntity.Supplier => ResolveSupplierAsync(context, id),
        DeletableEntity.Product => ResolveProductAsync(context, id),
        DeletableEntity.Remission => ResolveRemissionAsync(context, id),
        DeletableEntity.ReturnNote => ResolveReturnNoteAsync(context, id),
        DeletableEntity.Payment => ResolvePaymentAsync(context, id),
        _ => Task.FromResult(new DeletionSet()),
    };

    // Un cliente arrastra todos sus documentos: remisiones (con sus líneas), devoluciones
    // (propias o ligadas a alguna de esas remisiones) y pagos (con sus asignaciones).
    private static async Task<DeletionSet> ResolveCustomerAsync(LibrexDbContext context, int id)
    {
        var remissions = await context.Remissions.Where(r => r.CustomerId == id).ToListAsync();
        var remissionIds = remissions.Select(r => r.Id).ToList();

        var returnNotes = await context.ReturnNotes
            .Where(n => n.CustomerId == id || (n.RemissionId != null && remissionIds.Contains(n.RemissionId.Value)))
            .ToListAsync();
        var returnNoteIds = returnNotes.Select(n => n.Id).ToList();

        var payments = await context.Payments.Where(p => p.CustomerId == id).ToListAsync();
        var paymentIds = payments.Select(p => p.Id).ToList();

        return new DeletionSet
        {
            Remissions = remissions,
            RemissionDetails = await context.RemissionDetails
                .Where(d => remissionIds.Contains(d.RemissionId)).ToListAsync(),
            ReturnNotes = returnNotes,
            ReturnNoteDetails = await context.ReturnNoteDetails
                .Where(d => returnNoteIds.Contains(d.ReturnNoteId)).ToListAsync(),
            Payments = payments,
            PaymentAllocations = await context.PaymentAllocations
                .Where(a => paymentIds.Contains(a.PaymentId) || remissionIds.Contains(a.RemissionId)).ToListAsync(),
        };
    }

    // Un proveedor arrastra sus productos, y cada producto sus líneas de documento.
    // Los encabezados (remisiones, devoluciones, pagos) no se tocan.
    private static async Task<DeletionSet> ResolveSupplierAsync(LibrexDbContext context, int id)
    {
        var products = await context.Products.Where(p => p.SupplierId == id).ToListAsync();
        var productIds = products.Select(p => p.Id).ToList();

        return new DeletionSet
        {
            Products = products,
            RemissionDetails = await context.RemissionDetails
                .Where(d => productIds.Contains(d.ProductId)).ToListAsync(),
            ReturnNoteDetails = await context.ReturnNoteDetails
                .Where(d => productIds.Contains(d.ProductId)).ToListAsync(),
        };
    }

    private static async Task<DeletionSet> ResolveProductAsync(LibrexDbContext context, int id) => new()
    {
        RemissionDetails = await context.RemissionDetails.Where(d => d.ProductId == id).ToListAsync(),
        ReturnNoteDetails = await context.ReturnNoteDetails.Where(d => d.ProductId == id).ToListAsync(),
    };

    // Una remisión arrastra sus líneas, las asignaciones de pago que la apuntan y las
    // devoluciones ligadas a ella (RemissionId es nullable, pero se trata como cascada).
    private static async Task<DeletionSet> ResolveRemissionAsync(LibrexDbContext context, int id)
    {
        var returnNotes = await context.ReturnNotes.Where(n => n.RemissionId == id).ToListAsync();
        var returnNoteIds = returnNotes.Select(n => n.Id).ToList();

        return new DeletionSet
        {
            RemissionDetails = await context.RemissionDetails.Where(d => d.RemissionId == id).ToListAsync(),
            PaymentAllocations = await context.PaymentAllocations.Where(a => a.RemissionId == id).ToListAsync(),
            ReturnNotes = returnNotes,
            ReturnNoteDetails = await context.ReturnNoteDetails
                .Where(d => returnNoteIds.Contains(d.ReturnNoteId)).ToListAsync(),
        };
    }

    private static async Task<DeletionSet> ResolveReturnNoteAsync(LibrexDbContext context, int id) => new()
    {
        ReturnNoteDetails = await context.ReturnNoteDetails.Where(d => d.ReturnNoteId == id).ToListAsync(),
    };

    private static async Task<DeletionSet> ResolvePaymentAsync(LibrexDbContext context, int id) => new()
    {
        PaymentAllocations = await context.PaymentAllocations.Where(a => a.PaymentId == id).ToListAsync(),
    };
}
