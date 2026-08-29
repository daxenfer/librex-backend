namespace Librex.Application.DTOs.Reports;

// Vendido y devuelto van por separado (no se netean): un producto puede tener más devoluciones
// que ventas y aun así debe verse. Los arreglos van alineados con Products del reporte.
public record CustomerProductRowDto(
    int CustomerId,
    string CustomerName,
    IReadOnlyList<int> QuantitiesSold,
    IReadOnlyList<int> QuantitiesReturned,
    int TotalSold,
    int TotalReturned
);
