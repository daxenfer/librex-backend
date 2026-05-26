namespace Librex.Application.DTOs.Reports;

public record CustomerReportRowDto(
    int CustomerId,
    string CustomerName,
    decimal TotalSales,
    decimal TotalReturns,
    decimal TotalPayments,
    decimal Balance
);
