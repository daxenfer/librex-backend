using Librex.Domain.Entities;
using Librex.Domain.Enums;
using Librex.Domain.Interfaces;
using Librex.Infrastructure.Data;

namespace Librex.Infrastructure.Repositories;

public sealed class DeletionRepository(LibrexDbContext context) : IDeletionRepository
{
    public async Task<DeletionImpact?> GetImpactAsync(DeletableEntity entity, int id)
    {
        var label = await GetLabelAsync(entity, id);
        if (label is null) return null;

        var dependents = await DeletionGraph.ResolveAsync(context, entity, id);
        var preserved = await DeletionGraph.ResolvePreservedAsync(context, entity, id);
        return new DeletionImpact(entity, id, label, dependents.ToDependents(), preserved);
    }

    private async Task<string?> GetLabelAsync(DeletableEntity entity, int id) => entity switch
    {
        DeletableEntity.Customer => Active(await context.Customers.FindAsync(id))?.Name,
        DeletableEntity.Supplier => Active(await context.Suppliers.FindAsync(id))?.Name,
        DeletableEntity.Product => Active(await context.Products.FindAsync(id))?.Name,
        DeletableEntity.Remission => Folio(Active(await context.Remissions.FindAsync(id))?.FolioNumber),
        DeletableEntity.ReturnNote => Folio(Active(await context.ReturnNotes.FindAsync(id))?.FolioNumber),
        DeletableEntity.Payment => Folio(Active(await context.Payments.FindAsync(id))?.FolioNumber),
        _ => null,
    };

    // Un registro ya eliminado no tiene impacto que previsualizar: se trata como inexistente,
    // igual que en los GetById de cada repositorio.
    private static T? Active<T>(T? entity) where T : BaseEntity
        => entity is { IsActive: true } ? entity : null;

    private static string? Folio(int? folioNumber) => folioNumber is null ? null : $"Folio {folioNumber}";
}
