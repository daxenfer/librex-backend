using Librex.Domain.Enums;

namespace Librex.Domain.Interfaces;

// Cuántos registros de un tipo entran en un conteo de impacto.
public record DeletionDependent(DependentKind Kind, int Count);

// Lo que provoca eliminar una entidad raíz. Se calcula antes de borrar, para que el usuario
// confirme con el impacto a la vista.
//
// Dependents: lo que se marca como inactivo junto con la raíz.
// Preserved: documentos ya emitidos que la siguen citando y que NO se tocan — es lo que le
// asegura al usuario que su histórico (PDFs, reportes, saldos) queda intacto.
public record DeletionImpact(
    DeletableEntity Entity,
    int Id,
    string Label,
    IReadOnlyList<DeletionDependent> Dependents,
    IReadOnlyList<DeletionDependent> Preserved);

public interface IDeletionRepository
{
    // null si la entidad raíz no existe o ya fue eliminada.
    Task<DeletionImpact?> GetImpactAsync(DeletableEntity entity, int id);
}
