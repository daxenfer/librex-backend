namespace Librex.Application.DTOs.Reports;

public record CustomerProductRowDto(
    int CustomerId,
    string CustomerName,
    IReadOnlyList<int> Quantities,
    int TotalQuantity
);
