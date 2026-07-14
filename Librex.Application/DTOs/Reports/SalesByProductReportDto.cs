namespace Librex.Application.DTOs.Reports;

public record SalesByProductReportDto(
    int? SupplierId,
    string SupplierName,
    IReadOnlyList<ProductColumnDto> Products,
    IReadOnlyList<CustomerProductRowDto> Rows,
    IReadOnlyList<int> ProductTotals,
    int GrandTotal
);
