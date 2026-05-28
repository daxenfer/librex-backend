using Librex.Application.DTOs.Settings;
using Librex.Application.UseCases.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Librex.API.Controllers;

[ApiController]
[Route("api/settings")]
[Authorize]
public class SettingsController : ControllerBase
{
    private readonly ICompanySettingsService _service;

    public SettingsController(ICompanySettingsService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<CompanySettingsDto>> Get()
        => Ok(await _service.GetAsync());

    [HttpPut]
    public async Task<ActionResult<CompanySettingsDto>> Update([FromBody] UpdateCompanySettingsDto dto)
        => Ok(await _service.UpdateAsync(dto));
}
