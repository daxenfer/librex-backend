using Librex.Domain.Entities;
using Librex.Domain.Interfaces;
using Librex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Librex.Infrastructure.Repositories;

public class CompanySettingsRepository : ICompanySettingsRepository
{
    private readonly LibrexDbContext _context;

    public CompanySettingsRepository(LibrexDbContext context)
    {
        _context = context;
    }

    public async Task<CompanySettings> GetAsync()
    {
        var settings = await _context.CompanySettings.FirstOrDefaultAsync();
        if (settings is null)
        {
            settings = new CompanySettings
            {
                CompanyName = "Mi Empresa",
                BrandName = "Mi Empresa",
                Rfc = "RFC000000000",
            };
            _context.CompanySettings.Add(settings);
            await _context.SaveChangesAsync();
        }
        return settings;
    }

    public async Task UpdateAsync(CompanySettings settings)
    {
        _context.CompanySettings.Update(settings);
        await _context.SaveChangesAsync();
    }
}
