namespace Librex.Application.DTOs.Reports;

public record SupplierReportDto(
    int? SupplierId,
    string SupplierName,
    IEnumerable<CustomerReportRowDto> Customers,
    CustomerReportRowDto Totals
);
