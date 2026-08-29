using Librex.Domain.Enums;

namespace Librex.Domain.Interfaces;

// Cuántos registros de un tipo se van a borrar en cascada.
public record DeletionDependent(DependentKind Kind, int Count);

// Lo que se lleva por delante borrar una entidad raíz. Se calcula antes de borrar,
// para que el usuario confirme con el impacto a la vista.
public record DeletionImpact(
    DeletableEntity Entity,
    int Id,
    string Label,
    IReadOnlyList<DeletionDependent> Dependents);

public interface IDeletionRepository
{
    // null si la entidad raíz no existe.
    Task<DeletionImpact?> GetImpactAsync(DeletableEntity entity, int id);
}
