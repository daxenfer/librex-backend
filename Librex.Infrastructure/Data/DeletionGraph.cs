using Librex.Domain.Entities;
using Librex.Domain.Enums;
using Librex.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Librex.Infrastructure.Data;

// Registros dependientes que se marcan como inactivos junto con una entidad raíz.
// El borrado es lógico: nada se destruye, solo deja de existir para la aplicación.
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

    // Las entidades vienen trackeadas del context, así que basta con bajar la bandera; el
    // SaveChangesAsync del repositorio las persiste todas en una sola transacción. A diferencia
    // del borrado físico, aquí el orden no importa: no hay FKs de por medio.
    public void Deactivate()
    {
        foreach (var entity in All()) entity.IsActive = false;
    }

    private IEnumerable<BaseEntity> All() =>
    [
        .. PaymentAllocations.Cast<BaseEntity>(),
        .. Payments,
        .. ReturnNoteDetails,
        .. ReturnNotes,
        .. RemissionDetails,
        .. Remissions,
        .. Products,
    ];
}

// Única definición del grafo de dependencias del borrado. La usan tanto el conteo de impacto
// (DeletionRepository) como el borrado real (los DeleteAsync de cada repositorio), para que lo
// que se anuncia y lo que se elimina no puedan divergir.
//
// Regla de oro: nunca se toca un renglón cuyo encabezado sobrevive. Por eso eliminar un producto
// o un proveedor no arrastra renglones de remisión ni de devolución: mutilaría documentos ya
// emitidos, cambiando totales impresos y saldos de cuentas por cobrar.
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

    // Referencias que sobreviven: documentos ya emitidos que seguirán citando la entidad. Se
    // cuentan para poder decirle al usuario que su histórico queda intacto. Es el complemento
    // exacto de lo que ResolveAsync deliberadamente NO se lleva.
    public static async Task<IReadOnlyList<DeletionDependent>> ResolvePreservedAsync(
        LibrexDbContext context, DeletableEntity entity, int id)
    {
        List<int> productIds = entity switch
        {
            DeletableEntity.Product => [id],
            DeletableEntity.Supplier => await context.Products
                .Where(p => p.SupplierId == id)
                .Select(p => p.Id)
                .ToListAsync(),
            _ => [],
        };

        if (productIds.Count == 0) return [];

        // Documentos distintos, no renglones: al usuario le importa en cuántas remisiones
        // aparece el producto, no cuántas veces aparece dentro de cada una.
        var remissions = await context.RemissionDetails
            .Where(d => productIds.Contains(d.ProductId))
            .Select(d => d.RemissionId)
            .Distinct()
            .CountAsync();

        var returnNotes = await context.ReturnNoteDetails
            .Where(d => productIds.Contains(d.ProductId))
            .Select(d => d.ReturnNoteId)
            .Distinct()
            .CountAsync();

        return
        [
            .. new DeletionDependent[]
            {
                new(DependentKind.Remission, remissions),
                new(DependentKind.ReturnNote, returnNotes),
            }.Where(d => d.Count > 0)
        ];
    }

    // Un cliente arrastra todos sus documentos: remisiones (con sus líneas), devoluciones
    // (propias o ligadas a alguna de esas remisiones) y pagos (con sus asignaciones). Se van
    // documentos completos, así que ninguno queda mutilado.
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

    // Un proveedor arrastra sus productos, y nada más. Los renglones de remisión y devolución
    // que citan esos productos quedan intactos: pertenecen a documentos que sobreviven.
    private static async Task<DeletionSet> ResolveSupplierAsync(LibrexDbContext context, int id) => new()
    {
        Products = await context.Products.Where(p => p.SupplierId == id).ToListAsync(),
    };

    // Un producto no arrastra nada: sus renglones viven dentro de documentos que sobreviven.
    private static Task<DeletionSet> ResolveProductAsync(LibrexDbContext context, int id)
        => Task.FromResult(new DeletionSet());

    // Una remisión arrastra sus líneas, las asignaciones de pago que la apuntan y las
    // devoluciones ligadas a ella (RemissionId es nullable, pero se trata como cascada).
    // El pago sobrevive: al perder su asignación, su monto vuelve a quedar como anticipo.
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
