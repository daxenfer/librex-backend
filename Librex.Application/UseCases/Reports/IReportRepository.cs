using Librex.Application.DTOs.Reports;

namespace Librex.Application.UseCases.Reports;

public interface IReportRepository
{
    Task<SupplierReportDto> GetBySupplierAsync(int? supplierId);
    Task<SalesByProductReportDto> GetSalesByProductAsync(int? supplierId);
    Task<UnallocatedPaymentsReportDto> GetUnallocatedPaymentsAsync();
    Task<UnlinkedReturnsReportDto> GetUnlinkedReturnsAsync();
}
