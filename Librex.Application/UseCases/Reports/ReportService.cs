using Librex.Application.DTOs.Reports;
using Librex.Domain.Interfaces;

namespace Librex.Application.UseCases.Reports;

public sealed class ReportService(IReportRepository repository, ISupplierRepository suppliers) : IReportService
{
    public async Task<SupplierReportDto> GetBySupplierAsync(int? supplierId)
    {
        string supplierName = "Todas las editoriales";
        if (supplierId.HasValue)
        {
            var pub = await suppliers.GetByIdAsync(supplierId.Value);
            supplierName = pub?.Name ?? "Editorial desconocida";
        }

        return await repository.GetBySupplierAsync(supplierId) with
        {
            SupplierName = supplierName,
        };
    }

    public async Task<SalesByProductReportDto> GetSalesByProductAsync(int? supplierId)
    {
        string supplierName = "Todas las editoriales";
        if (supplierId.HasValue)
        {
            var pub = await suppliers.GetByIdAsync(supplierId.Value);
            supplierName = pub?.Name ?? "Editorial desconocida";
        }

        return await repository.GetSalesByProductAsync(supplierId) with
        {
            SupplierName = supplierName,
        };
    }

    public Task<UnallocatedPaymentsReportDto> GetUnallocatedPaymentsAsync()
        => repository.GetUnallocatedPaymentsAsync();

    public Task<UnlinkedReturnsReportDto> GetUnlinkedReturnsAsync()
        => repository.GetUnlinkedReturnsAsync();
}
