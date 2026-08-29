namespace Librex.Application.DTOs.Reports;

public record SalesByProductReportDto(
    int? SupplierId,
    string SupplierName,
    IReadOnlyList<ProductColumnDto> Products,
    IReadOnlyList<CustomerProductRowDto> Rows,
    IReadOnlyList<int> ProductTotalsSold,
    IReadOnlyList<int> ProductTotalsReturned,
    int GrandTotalSold,
    int GrandTotalReturned
);
