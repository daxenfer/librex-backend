using Librex.Application.DTOs.Settings;
using Librex.Domain.Entities;
using Librex.Domain.Interfaces;

namespace Librex.Application.UseCases.Settings;

public sealed class CompanySettingsService(ICompanySettingsRepository repository) : ICompanySettingsService
{
    public async Task<CompanySettingsDto> GetAsync(CancellationToken ct = default)
    {
        var settings = await repository.GetAsync(ct);
        return MapToDto(settings);
    }

    public async Task<CompanySettingsDto> UpdateAsync(UpdateCompanySettingsDto dto, CancellationToken ct = default)
    {
        var settings = await repository.GetAsync(ct);

        settings.CompanyName = dto.CompanyName;
        settings.BrandName = dto.BrandName;
        settings.Rfc = dto.Rfc;
        settings.Phone1 = dto.Phone1;
        settings.Phone2 = dto.Phone2;
        settings.Email = dto.Email;
        settings.Address = dto.Address;
        settings.PostalCode = dto.PostalCode;
        settings.City = dto.City;
        settings.State = dto.State;
        settings.LogoBase64 = dto.LogoBase64;

        await repository.UpdateAsync(settings, ct);
        return MapToDto(settings);
    }

    private static CompanySettingsDto MapToDto(CompanySettings s) => new()
    {
        Id = s.Id,
        CompanyName = s.CompanyName,
        BrandName = s.BrandName,
        Rfc = s.Rfc,
        Phone1 = s.Phone1,
        Phone2 = s.Phone2,
        Email = s.Email,
        Address = s.Address,
        PostalCode = s.PostalCode,
        City = s.City,
        State = s.State,
        LogoBase64 = s.LogoBase64,
    };
}
