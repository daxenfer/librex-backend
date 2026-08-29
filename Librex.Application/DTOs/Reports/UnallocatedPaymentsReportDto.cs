namespace Librex.Application.DTOs.Reports;

public record UnallocatedPaymentRowDto(
    int CustomerId,
    string CustomerName,
    decimal TotalPayments,
    decimal AllocatedAmount,
    decimal UnallocatedAmount
);

public record UnallocatedPaymentsReportDto(
    IEnumerable<UnallocatedPaymentRowDto> Rows,
    decimal TotalUnallocated
);
