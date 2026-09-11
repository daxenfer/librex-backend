using Librex.Application.DTOs.Reports;
using Librex.Application.UseCases.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Librex.API.Controllers;

[Authorize]
[ApiController]
[Route("api/reports")]
public sealed class ReportsController(IReportService service) : ControllerBase
{
    [HttpGet("by-supplier")]
    public async Task<ActionResult<SupplierReportDto>> BySupplier([FromQuery] int? supplierId)
        => Ok(await service.GetBySupplierAsync(supplierId));

    [HttpGet("sales-by-product")]
    public async Task<ActionResult<SalesByProductReportDto>> SalesByProduct([FromQuery] int? supplierId)
        => Ok(await service.GetSalesByProductAsync(supplierId));

    [HttpGet("unallocated-payments")]
    public async Task<ActionResult<UnallocatedPaymentsReportDto>> UnallocatedPayments()
        => Ok(await service.GetUnallocatedPaymentsAsync());

    [HttpGet("unlinked-returns")]
    public async Task<ActionResult<UnlinkedReturnsReportDto>> UnlinkedReturns()
        => Ok(await service.GetUnlinkedReturnsAsync());
}
