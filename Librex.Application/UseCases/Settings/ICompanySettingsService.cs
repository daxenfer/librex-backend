using Librex.Application.DTOs.Settings;

namespace Librex.Application.UseCases.Settings;

public interface ICompanySettingsService
{
    Task<CompanySettingsDto> GetAsync();
    Task<CompanySettingsDto> UpdateAsync(UpdateCompanySettingsDto dto);
}
