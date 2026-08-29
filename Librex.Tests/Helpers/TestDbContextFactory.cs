using Librex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Librex.Tests.Helpers;

// Crea un LibrexDbContext real sobre el proveedor InMemory, con una base distinta por test
// para que no se filtren datos entre pruebas.
internal static class TestDbContextFactory
{
    public static LibrexDbContext Create()
    {
        var options = new DbContextOptionsBuilder<LibrexDbContext>()
            .UseInMemoryDatabase($"librex-test-{Guid.NewGuid()}")
            .Options;

        return new LibrexDbContext(options);
    }
}
