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
    public async Task<ActionResult<SupplierReportDto>> BySupplier([FromQuery] int? supplierId, CancellationToken ct)
        => Ok(await service.GetBySupplierAsync(supplierId, ct));

    [HttpGet("sales-by-product")]
    public async Task<ActionResult<SalesByProductReportDto>> SalesByProduct([FromQuery] int? supplierId, CancellationToken ct)
        => Ok(await service.GetSalesByProductAsync(supplierId, ct));

    [HttpGet("unallocated-payments")]
    public async Task<ActionResult<UnallocatedPaymentsReportDto>> UnallocatedPayments(CancellationToken ct)
        => Ok(await service.GetUnallocatedPaymentsAsync(ct));

    [HttpGet("unlinked-returns")]
    public async Task<ActionResult<UnlinkedReturnsReportDto>> UnlinkedReturns(CancellationToken ct)
        => Ok(await service.GetUnlinkedReturnsAsync(ct));
}
