namespace Librex.Domain.Enums;

// Entidades raíz que se pueden borrar desde la API. Cada una arrastra sus dependientes en cascada.
public enum DeletableEntity
{
    Customer,
    Supplier,
    Product,
    Remission,
    ReturnNote,
    Payment,
}
