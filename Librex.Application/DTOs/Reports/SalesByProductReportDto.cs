namespace Librex.Application.DTOs.Reports;

public record SalesByProductReportDto(
    int? PublisherId,
    string PublisherName,
    IReadOnlyList<ProductColumnDto> Products,
    IReadOnlyList<CustomerProductRowDto> Rows,
    IReadOnlyList<int> ProductTotals,
    int GrandTotal
);
