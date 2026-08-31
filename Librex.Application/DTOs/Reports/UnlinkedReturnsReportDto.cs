namespace Librex.Application.DTOs.Reports;

// Devoluciones capturadas sin decir contra qué venta van. Espejo de UnallocatedPaymentsReportDto:
// igual que los anticipos, no se le atribuyen a ningún proveedor y se reportan aparte para que los
// saldos por proveedor cuadren.
public record UnlinkedReturnRowDto(
    int CustomerId,
    string CustomerName,
    int NoteCount,
    decimal UnlinkedAmount,
    string ReasonSummary
);

public record UnlinkedReturnsReportDto(
    IEnumerable<UnlinkedReturnRowDto> Rows,
    decimal TotalUnlinked
);
