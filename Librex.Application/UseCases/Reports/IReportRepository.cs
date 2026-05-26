using Librex.Application.DTOs.Reports;

namespace Librex.Application.UseCases.Reports;

public interface IReportRepository
{
    Task<PublisherReportDto> GetByPublisherAsync(int? publisherId, int tenantId);
    Task<SalesByProductReportDto> GetSalesByProductAsync(int? publisherId, int tenantId);
}
