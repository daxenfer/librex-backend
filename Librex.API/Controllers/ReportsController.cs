using Librex.Application.DTOs.Reports;
using Librex.Application.UseCases.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Librex.API.Controllers;

[Authorize]
[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _service;

    public ReportsController(IReportService service)
    {
        _service = service;
    }

    [HttpGet("by-publisher")]
    public async Task<ActionResult<PublisherReportDto>> ByPublisher([FromQuery] int? publisherId)
        => Ok(await _service.GetByPublisherAsync(publisherId));

    [HttpGet("sales-by-product")]
    public async Task<ActionResult<SalesByProductReportDto>> SalesByProduct([FromQuery] int? publisherId)
        => Ok(await _service.GetSalesByProductAsync(publisherId));
}
