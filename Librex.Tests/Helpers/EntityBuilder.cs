using Librex.Domain.Entities;

namespace Librex.Tests.Helpers;

// Factories de entidades para tests. Evita repetir object initializers largos en cada prueba.
internal static class EntityBuilder
{
    public static Customer NewCustomer(string name) => new()
    {
        Name = name,
        Address = "Calle 1",
        PostalCode = "64000",
        Phone = "8112345678",
        City = "Monterrey",
    };

    public static Supplier NewSupplier(string name) => new()
    {
        Name = name,
        Contact = "Contacto",
        Phone = "8187654321",
        Email = "contacto@proveedor.com",
    };

    public static Product NewProduct(Supplier supplier, string name) => new()
    {
        Name = name,
        Supplier = supplier,
    };

    public static Remission NewRemission(Customer customer, int folio, params RemissionDetail[] details) => new()
    {
        FolioNumber = folio,
        Customer = customer,
        Date = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        DeliveryDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        PaymentDueDate = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
        ReturnDueDate = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
        Details = [.. details],
    };

    public static RemissionDetail NewRemissionDetail(Product product, decimal quantity = 1m, decimal unitPrice = 100m) => new()
    {
        Product = product,
        Quantity = quantity,
        UnitPrice = unitPrice,
    };

    public static ReturnNote NewReturnNote(Customer customer, Remission? remission, int folio, params ReturnNoteDetail[] details) => new()
    {
        FolioNumber = folio,
        Customer = customer,
        Remission = remission,
        Date = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc),
        Details = [.. details],
    };

    public static ReturnNoteDetail NewReturnNoteDetail(Product product, decimal quantity = 1m, decimal unitPrice = 100m) => new()
    {
        Product = product,
        Quantity = quantity,
        UnitPrice = unitPrice,
    };

    public static Payment NewPayment(Customer customer, int folio, decimal amount, params PaymentAllocation[] allocations) => new()
    {
        FolioNumber = folio,
        Customer = customer,
        Date = new DateTime(2026, 1, 20, 0, 0, 0, DateTimeKind.Utc),
        Amount = amount,
        PaymentMethod = "Efectivo",
        Allocations = [.. allocations],
    };

    public static PaymentAllocation NewAllocation(Remission remission, decimal amount) => new()
    {
        Remission = remission,
        Amount = amount,
    };
}
