using Librex.Application.DTOs.Reports;

namespace Librex.Application.UseCases.Reports;

public interface IReportService
{
    Task<SupplierReportDto> GetBySupplierAsync(int? supplierId);
    Task<SalesByProductReportDto> GetSalesByProductAsync(int? supplierId);
}
