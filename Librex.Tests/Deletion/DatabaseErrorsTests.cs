using Librex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Librex.Tests.Deletion;

// El detalle útil de una violación de FK no viene en la excepción de EF, sino en la
// PostgresException interna. Sin esto, un borrado que choca con una FK sale como 500 opaco.
public class DatabaseErrorsTests
{
    private static PostgresException Postgres(string sqlState)
        => new("violates foreign key constraint", "ERROR", "ERROR", sqlState);

    [Fact]
    public void IsForeignKeyViolation_DbUpdateExceptionWithSqlState23503_ReturnsTrue()
    {
        var exception = new DbUpdateException("update failed", Postgres("23503"));

        Assert.True(DatabaseErrors.IsForeignKeyViolation(exception));
    }

    [Fact]
    public void IsForeignKeyViolation_NestedSeveralLevelsDeep_ReturnsTrue()
    {
        var exception = new InvalidOperationException("wrapper",
            new DbUpdateException("update failed", Postgres("23503")));

        Assert.True(DatabaseErrors.IsForeignKeyViolation(exception));
    }

    [Fact]
    public void IsForeignKeyViolation_UniqueViolation_ReturnsFalse()
    {
        var exception = new DbUpdateException("update failed", Postgres("23505"));

        Assert.False(DatabaseErrors.IsForeignKeyViolation(exception));
    }

    [Fact]
    public void IsForeignKeyViolation_UnrelatedException_ReturnsFalse()
    {
        Assert.False(DatabaseErrors.IsForeignKeyViolation(new InvalidOperationException("boom")));
    }
}
