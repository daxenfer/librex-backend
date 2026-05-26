using Librex.Domain.Entities;
using Librex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Librex.Infrastructure.Data;

public static class DatabaseInitializer
{
    public static async Task SeedAsync(LibrexDbContext context)
    {
        await context.Database.MigrateAsync();

        if (!await context.Users.AnyAsync())
        {
            context.Users.Add(new User
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin1234"),
                FullName = "System Administrator",
                Role = "Administrator",
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                TenantId = 1,
            });
            await context.SaveChangesAsync();
        }
    }
}
