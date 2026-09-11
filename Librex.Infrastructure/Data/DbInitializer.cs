using Librex.Domain.Constants;
using Librex.Domain.Entities;
using Librex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Librex.Infrastructure.Data;

public static class DatabaseInitializer
{
    private const string SeedUsername = "superadmin";

    // Contraseña del usuario semilla. Que sea larga y con símbolos ayuda contra un ataque de
    // fuerza bruta desde el login, pero una contraseña escrita en el código es pública para
    // cualquiera que tenga el repositorio: sirve para arrancar una instalación nueva, no para
    // dejarla puesta. Para un despliegue real se pisa sin tocar el código con
    //   dotnet user-secrets set "Seed:AdminPassword" "..."   --project Librex.API
    // o con la variable de entorno Seed__AdminPassword.
    private const string DefaultSeedPassword = "Admin1234!*";

    public static async Task SeedAsync(LibrexDbContext context, string? adminPassword = null)
    {
        await context.Database.MigrateAsync();

        if (!await context.Users.AnyAsync())
        {
            var password = string.IsNullOrWhiteSpace(adminPassword) ? DefaultSeedPassword : adminPassword;

            context.Users.Add(new User
            {
                Username = SeedUsername,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                FullName = "System Administrator",
                Role = Roles.SuperAdmin,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
            });
            await context.SaveChangesAsync();
            return;
        }

        // Instalaciones creadas antes de que existieran los roles: su usuario semilla quedó como
        // Administrator y, sin esto, nadie podría entrar a administrar usuarios. Solo corre cuando
        // no hay NINGÚN SuperAdmin activo, así que después del primer arranque es un no-op.
        if (!await context.Users.AnyAsync(u => u.Role == Roles.SuperAdmin && u.IsActive))
        {
            var seedUser = await context.Users.FirstOrDefaultAsync(u => u.Username == SeedUsername && u.IsActive);
            if (seedUser is not null)
            {
                seedUser.Role = Roles.SuperAdmin;
                seedUser.ModifiedAt = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }
        }
    }
}
