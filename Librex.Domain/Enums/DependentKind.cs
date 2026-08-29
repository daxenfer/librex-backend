namespace Librex.Domain.Enums;

// Tipos de registro que pueden caer al borrar una entidad raíz.
public enum DependentKind
{
    Product,
    Remission,
    RemissionDetail,
    ReturnNote,
    ReturnNoteDetail,
    Payment,
    PaymentAllocation,
}
