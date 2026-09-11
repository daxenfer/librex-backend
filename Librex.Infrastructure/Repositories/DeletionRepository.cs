using Librex.Domain.Entities;
using Librex.Domain.Enums;
using Librex.Domain.Interfaces;
using Librex.Infrastructure.Data;

namespace Librex.Infrastructure.Repositories;

public sealed class DeletionRepository(LibrexDbContext context) : IDeletionRepository
{
    public async Task<DeletionImpact?> GetImpactAsync(DeletableEntity entity, int id, CancellationToken ct = default)
    {
        var label = await GetLabelAsync(entity, id, ct);
        if (label is null) return null;

        var dependents = await DeletionGraph.ResolveAsync(context, entity, id, ct);
        var preserved = await DeletionGraph.ResolvePreservedAsync(context, entity, id, ct);
        return new DeletionImpact(entity, id, label, dependents.ToDependents(), preserved);
    }

    private async Task<string?> GetLabelAsync(DeletableEntity entity, int id, CancellationToken ct = default) => entity switch
    {
        DeletableEntity.Customer => Active(await context.Customers.FindAsync([id], ct))?.Name,
        DeletableEntity.Supplier => Active(await context.Suppliers.FindAsync([id], ct))?.Name,
        DeletableEntity.Product => Active(await context.Products.FindAsync([id], ct))?.Name,
        DeletableEntity.Remission => FolioLabel(Active(await context.Remissions.FindAsync([id], ct))?.FolioNumber),
        DeletableEntity.ReturnNote => FolioLabel(Active(await context.ReturnNotes.FindAsync([id], ct))?.FolioNumber),
        DeletableEntity.Payment => FolioLabel(Active(await context.Payments.FindAsync([id], ct))?.FolioNumber),
        _ => null,
    };

    // Un registro ya eliminado no tiene impacto que previsualizar: se trata como inexistente,
    // igual que en los GetById de cada repositorio.
    private static T? Active<T>(T? entity) where T : BaseEntity
        => entity is { IsActive: true } ? entity : null;

    private static string? FolioLabel(int? folioNumber)
        => folioNumber is null ? null : $"Folio {Domain.Entities.Folio.Format(folioNumber)}";
}
