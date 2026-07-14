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

    [HttpGet("by-supplier")]
    public async Task<ActionResult<SupplierReportDto>> BySupplier([FromQuery] int? supplierId)
        => Ok(await _service.GetBySupplierAsync(supplierId));

    [HttpGet("sales-by-product")]
    public async Task<ActionResult<SalesByProductReportDto>> SalesByProduct([FromQuery] int? supplierId)
        => Ok(await _service.GetSalesByProductAsync(supplierId));
}
