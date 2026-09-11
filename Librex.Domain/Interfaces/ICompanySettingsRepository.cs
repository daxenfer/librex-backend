using Librex.Domain.Entities;

namespace Librex.Domain.Interfaces;

public interface ICompanySettingsRepository
{
    Task<CompanySettings> GetAsync(CancellationToken ct = default);
    Task UpdateAsync(CompanySettings settings, CancellationToken ct = default);
}
