using Npgsql;

namespace Librex.Infrastructure.Data;

// Traduce errores crudos de Postgres a algo que las capas de arriba puedan decidir.
// Vive en Infrastructure porque los códigos SQLSTATE son detalle del motor.
public static class DatabaseErrors
{
    // 23503 = foreign_key_violation. Se intentó borrar (o insertar) algo que rompe una FK.
    private const string ForeignKeyViolation = "23503";

    public static bool IsForeignKeyViolation(Exception? exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is PostgresException postgres && postgres.SqlState == ForeignKeyViolation)
                return true;
        }

        return false;
    }
}
