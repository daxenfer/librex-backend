using Librex.Domain.Entities;
using Librex.Domain.Interfaces;
using Librex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Librex.Infrastructure.Repositories;

public sealed class CompanySettingsRepository(LibrexDbContext context) : ICompanySettingsRepository
{
    public async Task<CompanySettings> GetAsync()
    {
        var settings = await context.CompanySettings.FirstOrDefaultAsync();
        if (settings is null)
        {
            settings = new CompanySettings
            {
                CompanyName = "Mi Empresa",
                BrandName = "Mi Empresa",
                Rfc = "RFC000000000",
            };
            context.CompanySettings.Add(settings);
            await context.SaveChangesAsync();
        }
        return settings;
    }

    public async Task UpdateAsync(CompanySettings settings)
    {
        context.CompanySettings.Update(settings);
        await context.SaveChangesAsync();
    }
}
