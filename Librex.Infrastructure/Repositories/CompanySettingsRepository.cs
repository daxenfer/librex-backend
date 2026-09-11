using Librex.Domain.Entities;
using Librex.Domain.Interfaces;
using Librex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Librex.Infrastructure.Repositories;

public sealed class CompanySettingsRepository(LibrexDbContext context) : ICompanySettingsRepository
{
    public async Task<CompanySettings> GetAsync(CancellationToken ct = default)
    {
        var settings = await context.CompanySettings.FirstOrDefaultAsync(ct);
        if (settings is null)
        {
            settings = new CompanySettings
            {
                CompanyName = "Mi Empresa",
                BrandName = "Mi Empresa",
                Rfc = "RFC000000000",
            };
            context.CompanySettings.Add(settings);
            await context.SaveChangesAsync(ct);
        }
        return settings;
    }

    public async Task UpdateAsync(CompanySettings settings, CancellationToken ct = default)
    {
        context.CompanySettings.Update(settings);
        await context.SaveChangesAsync(ct);
    }
}
