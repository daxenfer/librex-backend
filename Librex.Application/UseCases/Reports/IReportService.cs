using Librex.Application.DTOs.Reports;

namespace Librex.Application.UseCases.Reports;

public interface IReportService
{
    Task<SupplierReportDto> GetBySupplierAsync(int? supplierId, CancellationToken ct = default);
    Task<SalesByProductReportDto> GetSalesByProductAsync(int? supplierId, CancellationToken ct = default);
    Task<UnallocatedPaymentsReportDto> GetUnallocatedPaymentsAsync(CancellationToken ct = default);
    Task<UnlinkedReturnsReportDto> GetUnlinkedReturnsAsync(CancellationToken ct = default);
}
