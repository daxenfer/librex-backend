using Librex.Domain.Entities;
using Librex.Infrastructure.Data;

namespace Librex.Tests.Helpers;

// Escenario compartido por los tests de borrado. Siembra dos clientes con documentos propios,
// para poder verificar tanto la cascada del cliente objetivo como que el otro queda intacto.
//
//   Supplier S     -> Product P1, Product P2
//   Customer C1    -> Remission R1 (P1, P2), Remission R2 (P1)
//                     ReturnNote RN1 (ligada a R1, con P1)
//                     Payment PM1 (asignado a R1 y R2)
//   Customer C2    -> Remission R3 (P2), ReturnNote RN2 (ligada a R3, con P2), Payment PM2 (a R3)
internal sealed class DeletionScenario
{
    public required Supplier Supplier { get; init; }
    public required Product Product1 { get; init; }
    public required Product Product2 { get; init; }
    public required Customer Customer1 { get; init; }
    public required Customer Customer2 { get; init; }
    public required Remission Remission1 { get; init; }
    public required Remission Remission2 { get; init; }
    public required Remission Remission3 { get; init; }
    public required ReturnNote ReturnNote1 { get; init; }
    public required ReturnNote ReturnNote2 { get; init; }
    public required Payment Payment1 { get; init; }
    public required Payment Payment2 { get; init; }

    public static async Task<DeletionScenario> SeedAsync(LibrexDbContext context)
    {
        var supplier = EntityBuilder.NewSupplier("Editorial Norte");
        var product1 = EntityBuilder.NewProduct(supplier, "Matemáticas 1");
        var product2 = EntityBuilder.NewProduct(supplier, "Español 1");

        var customer1 = EntityBuilder.NewCustomer("Escuela Central");
        var remission1 = EntityBuilder.NewRemission(customer1, 1,
            EntityBuilder.NewRemissionDetail(product1),
            EntityBuilder.NewRemissionDetail(product2));
        var remission2 = EntityBuilder.NewRemission(customer1, 2,
            EntityBuilder.NewRemissionDetail(product1));
        var returnNote1 = EntityBuilder.NewReturnNote(customer1, remission1, 1,
            EntityBuilder.NewReturnNoteDetail(product1));
        var payment1 = EntityBuilder.NewPayment(customer1, 1, 500m,
            EntityBuilder.NewAllocation(remission1, 200m),
            EntityBuilder.NewAllocation(remission2, 100m));

        var customer2 = EntityBuilder.NewCustomer("Escuela Sur");
        var remission3 = EntityBuilder.NewRemission(customer2, 3,
            EntityBuilder.NewRemissionDetail(product2));
        var returnNote2 = EntityBuilder.NewReturnNote(customer2, remission3, 2,
            EntityBuilder.NewReturnNoteDetail(product2));
        var payment2 = EntityBuilder.NewPayment(customer2, 2, 50m,
            EntityBuilder.NewAllocation(remission3, 50m));

        context.AddRange(supplier, product1, product2,
            customer1, remission1, remission2, returnNote1, payment1,
            customer2, remission3, returnNote2, payment2);
        await context.SaveChangesAsync();

        return new DeletionScenario
        {
            Supplier = supplier,
            Product1 = product1,
            Product2 = product2,
            Customer1 = customer1,
            Customer2 = customer2,
            Remission1 = remission1,
            Remission2 = remission2,
            Remission3 = remission3,
            ReturnNote1 = returnNote1,
            ReturnNote2 = returnNote2,
            Payment1 = payment1,
            Payment2 = payment2,
        };
    }
}
