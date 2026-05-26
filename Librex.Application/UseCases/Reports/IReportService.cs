using Librex.Application.DTOs.Reports;

namespace Librex.Application.UseCases.Reports;

public interface IReportService
{
    Task<PublisherReportDto> GetByPublisherAsync(int? publisherId);
    Task<SalesByProductReportDto> GetSalesByProductAsync(int? publisherId);
}
