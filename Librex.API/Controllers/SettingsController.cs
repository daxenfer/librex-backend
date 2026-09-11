using Librex.Application.DTOs.Settings;
using Librex.Application.UseCases.Settings;
using Librex.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Librex.API.Controllers;

[ApiController]
[Route("api/settings")]
[Authorize]
public sealed class SettingsController(ICompanySettingsService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CompanySettingsDto>> Get(CancellationToken ct)
        => Ok(await service.GetAsync(ct));

    [HttpPut]
    [Authorize(Policy = Permissions.SettingsManage)]
    public async Task<ActionResult<CompanySettingsDto>> Update([FromBody] UpdateCompanySettingsDto dto, CancellationToken ct)
        => Ok(await service.UpdateAsync(dto, ct));
}
