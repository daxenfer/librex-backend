using Librex.Domain.Entities;

namespace Librex.Domain.Interfaces;

public interface ICompanySettingsRepository
{
    Task<CompanySettings> GetAsync();
    Task UpdateAsync(CompanySettings settings);
}
