using Librex.Application.DTOs.Reports;
using Librex.Domain.Interfaces;

namespace Librex.Application.UseCases.Reports;

public sealed class ReportService(IReportRepository repository, ISupplierRepository suppliers) : IReportService
{
    public async Task<SupplierReportDto> GetBySupplierAsync(int? supplierId, CancellationToken ct = default)
    {
        string supplierName = "Todas las editoriales";
        if (supplierId.HasValue)
        {
            var pub = await suppliers.GetByIdAsync(supplierId.Value, ct);
            supplierName = pub?.Name ?? "Editorial desconocida";
        }

        return await repository.GetBySupplierAsync(supplierId, ct) with
        {
            SupplierName = supplierName,
        };
    }

    public async Task<SalesByProductReportDto> GetSalesByProductAsync(int? supplierId, CancellationToken ct = default)
    {
        string supplierName = "Todas las editoriales";
        if (supplierId.HasValue)
        {
            var pub = await suppliers.GetByIdAsync(supplierId.Value, ct);
            supplierName = pub?.Name ?? "Editorial desconocida";
        }

        return await repository.GetSalesByProductAsync(supplierId, ct) with
        {
            SupplierName = supplierName,
        };
    }

    public Task<UnallocatedPaymentsReportDto> GetUnallocatedPaymentsAsync(CancellationToken ct = default)
        => repository.GetUnallocatedPaymentsAsync(ct);

    public Task<UnlinkedReturnsReportDto> GetUnlinkedReturnsAsync(CancellationToken ct = default)
        => repository.GetUnlinkedReturnsAsync(ct);
}
