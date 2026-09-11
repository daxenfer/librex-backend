using Librex.Application.DTOs.Settings;

namespace Librex.Application.UseCases.Settings;

public interface ICompanySettingsService
{
    Task<CompanySettingsDto> GetAsync(CancellationToken ct = default);
    Task<CompanySettingsDto> UpdateAsync(UpdateCompanySettingsDto dto, CancellationToken ct = default);
}
