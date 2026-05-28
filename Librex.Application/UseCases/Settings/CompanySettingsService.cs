using Librex.Application.DTOs.Settings;
using Librex.Domain.Entities;
using Librex.Domain.Interfaces;

namespace Librex.Application.UseCases.Settings;

public class CompanySettingsService : ICompanySettingsService
{
    private readonly ICompanySettingsRepository _repository;

    public CompanySettingsService(ICompanySettingsRepository repository)
    {
        _repository = repository;
    }

    public async Task<CompanySettingsDto> GetAsync()
    {
        var settings = await _repository.GetAsync();
        return MapToDto(settings);
    }

    public async Task<CompanySettingsDto> UpdateAsync(UpdateCompanySettingsDto dto)
    {
        var settings = await _repository.GetAsync();

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

        await _repository.UpdateAsync(settings);
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
    };
}
