namespace Librex.Application.DTOs.Reports;

public record PublisherReportDto(
    int? PublisherId,
    string PublisherName,
    IEnumerable<CustomerReportRowDto> Customers,
    CustomerReportRowDto Totals
);
