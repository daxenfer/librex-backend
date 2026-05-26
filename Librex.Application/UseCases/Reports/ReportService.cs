using Librex.Application.DTOs.Reports;
using Librex.Domain.Interfaces;

namespace Librex.Application.UseCases.Reports;

public class ReportService : IReportService
{
    private readonly IReportRepository _repository;
    private readonly IPublisherRepository _publishers;

    public ReportService(IReportRepository repository, IPublisherRepository publishers)
    {
        _repository = repository;
        _publishers = publishers;
    }

    public async Task<PublisherReportDto> GetByPublisherAsync(int? publisherId)
    {
        string publisherName = "Todas las editoriales";
        if (publisherId.HasValue)
        {
            var pub = await _publishers.GetByIdAsync(publisherId.Value);
            publisherName = pub?.Name ?? "Editorial desconocida";
        }

        return await _repository.GetByPublisherAsync(publisherId, tenantId: 1) with
        {
            PublisherName = publisherName,
        };
    }

    public async Task<SalesByProductReportDto> GetSalesByProductAsync(int? publisherId)
    {
        string publisherName = "Todas las editoriales";
        if (publisherId.HasValue)
        {
            var pub = await _publishers.GetByIdAsync(publisherId.Value);
            publisherName = pub?.Name ?? "Editorial desconocida";
        }

        return await _repository.GetSalesByProductAsync(publisherId, tenantId: 1) with
        {
            PublisherName = publisherName,
        };
    }
}
