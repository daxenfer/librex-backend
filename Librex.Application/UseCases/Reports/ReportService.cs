using Librex.Application.DTOs.Reports;
using Librex.Domain.Interfaces;

namespace Librex.Application.UseCases.Reports;

public class ReportService : IReportService
{
    private readonly IReportRepository _repository;
    private readonly ISupplierRepository _suppliers;

    public ReportService(IReportRepository repository, ISupplierRepository suppliers)
    {
        _repository = repository;
        _suppliers = suppliers;
    }

    public async Task<SupplierReportDto> GetBySupplierAsync(int? supplierId)
    {
        string supplierName = "Todos los proveedores";
        if (supplierId.HasValue)
        {
            var pub = await _suppliers.GetByIdAsync(supplierId.Value);
            supplierName = pub?.Name ?? "Proveedor desconocido";
        }

        return await _repository.GetBySupplierAsync(supplierId) with
        {
            SupplierName = supplierName,
        };
    }

    public async Task<SalesByProductReportDto> GetSalesByProductAsync(int? supplierId)
    {
        string supplierName = "Todos los proveedores";
        if (supplierId.HasValue)
        {
            var pub = await _suppliers.GetByIdAsync(supplierId.Value);
            supplierName = pub?.Name ?? "Proveedor desconocido";
        }

        return await _repository.GetSalesByProductAsync(supplierId) with
        {
            SupplierName = supplierName,
        };
    }

    public Task<UnallocatedPaymentsReportDto> GetUnallocatedPaymentsAsync()
        => _repository.GetUnallocatedPaymentsAsync();

    public Task<UnlinkedReturnsReportDto> GetUnlinkedReturnsAsync()
        => _repository.GetUnlinkedReturnsAsync();
}
