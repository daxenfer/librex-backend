using Librex.Application.DTOs.Deletion;
using Librex.Domain.Enums;
using Librex.Domain.Interfaces;

namespace Librex.Application.UseCases.Deletion;

public sealed class DeletionService(IDeletionRepository repository) : IDeletionService
{
    public async Task<DeletionImpactDto?> GetImpactAsync(DeletableEntity entity, int id, CancellationToken ct = default)
    {
        var impact = await repository.GetImpactAsync(entity, id, ct);
        return impact is null ? null : MapToDto(impact);
    }

    private static DeletionImpactDto MapToDto(DeletionImpact impact) => new()
    {
        EntityType = impact.Entity.ToString(),
        Id = impact.Id,
        Label = impact.Label,
        Items = MapItems(impact.Dependents),
        TotalDependents = impact.Dependents.Sum(d => d.Count),
        PreservedItems = MapItems(impact.Preserved),
        TotalPreserved = impact.Preserved.Sum(d => d.Count),
    };

    private static List<DeletionImpactItemDto> MapItems(IReadOnlyList<DeletionDependent> dependents)
        => [.. dependents.Select(d => new DeletionImpactItemDto
        {
            EntityName = NameOf(d.Kind),
            Count = d.Count,
        })];

    private static string NameOf(DependentKind kind) => kind switch
    {
        DependentKind.Product => "Productos",
        DependentKind.Remission => "Remisiones",
        DependentKind.RemissionDetail => "Líneas de remisión",
        DependentKind.ReturnNote => "Devoluciones",
        DependentKind.ReturnNoteDetail => "Líneas de devolución",
        DependentKind.Payment => "Pagos",
        DependentKind.PaymentAllocation => "Aplicaciones de pago",
        _ => kind.ToString(),
    };
}
