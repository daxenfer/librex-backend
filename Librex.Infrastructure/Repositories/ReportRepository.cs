using Librex.Application.DTOs.Reports;
using Librex.Application.UseCases.Reports;
using Librex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Librex.Infrastructure.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly LibrexDbContext _context;

    public ReportRepository(LibrexDbContext context)
    {
        _context = context;
    }

    public async Task<SupplierReportDto> GetBySupplierAsync(int? supplierId)
    {
        var salesQuery = _context.RemissionDetails
            .Where(d => d.IsActive && d.Remission.IsActive);
        if (supplierId.HasValue)
            salesQuery = salesQuery.Where(d => d.Product.SupplierId == supplierId.Value);

        var byRemission = await salesQuery
            .GroupBy(d => new {
                d.Remission.CustomerId,
                CustomerName = d.Remission.Customer.Name,
                d.RemissionId,
                Discount = d.Remission.Discount
            })
            .Select(g => new {
                g.Key.CustomerId, g.Key.CustomerName, g.Key.RemissionId,
                Subtotal = g.Sum(d => d.Quantity * d.UnitPrice),
                g.Key.Discount
            })
            .ToListAsync();

        // Subtotal completo por remisión (sin filtrar por proveedor) para prorratear
        // el descuento, que ahora es un monto fijo de la remisión.
        var fullSubtotals = await _context.RemissionDetails
            .Where(d => d.IsActive && d.Remission.IsActive)
            .GroupBy(d => d.RemissionId)
            .Select(g => new { RemissionId = g.Key, Subtotal = g.Sum(d => d.Quantity * d.UnitPrice) })
            .ToDictionaryAsync(x => x.RemissionId, x => x.Subtotal);

        var sales = byRemission
            .GroupBy(r => new { r.CustomerId, r.CustomerName })
            .Select(g => new {
                g.Key.CustomerId, Name = g.Key.CustomerName,
                Total = g.Sum(r =>
                {
                    var fullSubtotal = fullSubtotals.GetValueOrDefault(r.RemissionId);
                    // Reparte el descuento (monto fijo) según la participación del proveedor en la remisión.
                    var appliedDiscount = fullSubtotal == 0 ? 0m : r.Discount * (r.Subtotal / fullSubtotal);
                    return r.Subtotal - appliedDiscount;
                })
            }).ToList();

        var returnsQuery = _context.ReturnNoteDetails
            .Where(d => d.IsActive && d.ReturnNote.IsActive);
        if (supplierId.HasValue)
            // Mismo criterio que ya se aplica a los pagos: solo lo ligado a una remisión se le
            // atribuye a un proveedor. Las devoluciones sueltas se reportan aparte, con
            // GetUnlinkedReturnsAsync, para que los totales cuadren.
            returnsQuery = returnsQuery
                .Where(d => d.Product.SupplierId == supplierId.Value)
                .Where(d => d.ReturnNote.RemissionId != null);

        var returns = await returnsQuery
            .GroupBy(d => new { d.ReturnNote.CustomerId, Name = d.ReturnNote.Customer.Name })
            .Select(g => new { g.Key.CustomerId, g.Key.Name, Total = g.Sum(d => d.Quantity * d.UnitPrice) })
            .ToListAsync();

        Dictionary<int, (string Name, decimal Total)> paymentsDict;

        if (supplierId.HasValue)
        {
            // Las asignaciones de pago llevan el monto aplicado a cada remisión.
            var allocations = await _context.PaymentAllocations
                .Where(a => a.IsActive && a.Payment.IsActive)
                .Select(a => new {
                    a.RemissionId,
                    a.Amount,
                    a.Payment.CustomerId,
                    CustomerName = a.Payment.Customer.Name
                })
                .ToListAsync();

            var remissionShares = await _context.RemissionDetails
                .Where(d => d.IsActive && d.Remission.IsActive)
                .GroupBy(d => new { d.RemissionId, d.Product.SupplierId })
                .Select(g => new {
                    g.Key.RemissionId,
                    g.Key.SupplierId,
                    Amount = g.Sum(d => d.Quantity * d.UnitPrice)
                })
                .ToListAsync();

            var remissionTotals = remissionShares
                .GroupBy(x => x.RemissionId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

            var supplierAmountByRemission = remissionShares
                .Where(x => x.SupplierId == supplierId.Value)
                .ToDictionary(x => x.RemissionId, x => x.Amount);

            // Prorratea cada asignación por la participación del proveedor en la remisión.
            // Los anticipos (no asignados) no se atribuyen a ningún proveedor; se reportan
            // aparte con GetUnallocatedPaymentsAsync para que los totales cuadren.
            paymentsDict = allocations
                .GroupBy(a => new { a.CustomerId, a.CustomerName })
                .ToDictionary(
                    g => g.Key.CustomerId,
                    g => (
                        Name: g.Key.CustomerName,
                        Total: g.Sum(a =>
                        {
                            var remTotal = remissionTotals.GetValueOrDefault(a.RemissionId);
                            if (remTotal == 0) return 0m;
                            var supAmt = supplierAmountByRemission.GetValueOrDefault(a.RemissionId);
                            return a.Amount * (supAmt / remTotal);
                        })
                    )
                );
        }
        else
        {
            // A nivel cliente, todo el dinero recibido (incluye anticipos) reduce el saldo.
            var rawPayments = await _context.Payments
                .Where(p => p.IsActive)
                .Select(p => new { p.CustomerId, CustomerName = p.Customer.Name, p.Amount })
                .ToListAsync();

            paymentsDict = rawPayments
                .GroupBy(p => new { p.CustomerId, p.CustomerName })
                .ToDictionary(
                    g => g.Key.CustomerId,
                    g => (Name: g.Key.CustomerName, Total: g.Sum(p => p.Amount))
                );
        }

        var allCustomerIds = sales.Select(s => s.CustomerId)
            .Union(returns.Select(r => r.CustomerId))
            .Union(paymentsDict.Keys)
            .Distinct();

        var salesDict = sales.ToDictionary(s => s.CustomerId, s => (s.Name, s.Total));
        var returnsDict = returns.ToDictionary(r => r.CustomerId, r => r.Total);

        var rows = allCustomerIds
            .Select(cid =>
            {
                salesDict.TryGetValue(cid, out var s);
                returnsDict.TryGetValue(cid, out var ret);
                paymentsDict.TryGetValue(cid, out var pay);
                var name = s.Name ?? pay.Name ?? string.Empty;
                var totalSales = s.Total;
                var totalReturns = ret;
                var totalPayments = pay.Total;
                return new CustomerReportRowDto(cid, name, totalSales, totalReturns, totalPayments,
                    totalSales - totalReturns - totalPayments);
            })
            .Where(r => r.TotalSales > 0 || r.TotalReturns > 0 || r.TotalPayments > 0)
            .OrderBy(r => r.CustomerName)
            .ToList();

        var totals = new CustomerReportRowDto(
            0, "TOTALES",
            rows.Sum(r => r.TotalSales),
            rows.Sum(r => r.TotalReturns),
            rows.Sum(r => r.TotalPayments),
            rows.Sum(r => r.Balance)
        );

        return new SupplierReportDto(supplierId, string.Empty, rows, totals);
    }

    public async Task<SalesByProductReportDto> GetSalesByProductAsync(int? supplierId)
    {
        var salesQuery = _context.RemissionDetails
            .Where(d => d.IsActive && d.Remission.IsActive);
        if (supplierId.HasValue)
            salesQuery = salesQuery.Where(d => d.Product.SupplierId == supplierId.Value);

        var sales = await salesQuery
            .GroupBy(d => new { d.Remission.CustomerId, CustomerName = d.Remission.Customer.Name,
                                d.ProductId, ProductName = d.Product.Name })
            .Select(g => new { g.Key.CustomerId, g.Key.CustomerName,
                               g.Key.ProductId, g.Key.ProductName, Qty = (int)g.Sum(d => d.Quantity) })
            .ToListAsync();

        var returnsQuery = _context.ReturnNoteDetails
            .Where(d => d.IsActive && d.ReturnNote.IsActive);
        if (supplierId.HasValue)
            returnsQuery = returnsQuery.Where(d => d.Product.SupplierId == supplierId.Value);

        var returns = await returnsQuery
            .GroupBy(d => new { d.ReturnNote.CustomerId, CustomerName = d.ReturnNote.Customer.Name,
                                d.ProductId, ProductName = d.Product.Name })
            .Select(g => new { g.Key.CustomerId, g.Key.CustomerName,
                               g.Key.ProductId, g.Key.ProductName, Qty = (int)g.Sum(d => d.Quantity) })
            .ToListAsync();

        // Vendido y devuelto por separado, por (customerId, productId). No se netean: un producto
        // debe verse aunque se haya devuelto más de lo vendido.
        var soldDict = new Dictionary<(int, int), int>();
        var returnedDict = new Dictionary<(int, int), int>();
        var customerNames = new Dictionary<int, string>();
        var productNames = new Dictionary<int, string>();

        foreach (var s in sales)
        {
            var key = (s.CustomerId, s.ProductId);
            soldDict[key] = soldDict.GetValueOrDefault(key) + s.Qty;
            customerNames.TryAdd(s.CustomerId, s.CustomerName);
            productNames.TryAdd(s.ProductId, s.ProductName);
        }
        foreach (var r in returns)
        {
            var key = (r.CustomerId, r.ProductId);
            returnedDict[key] = returnedDict.GetValueOrDefault(key) + r.Qty;
            customerNames.TryAdd(r.CustomerId, r.CustomerName);
            productNames.TryAdd(r.ProductId, r.ProductName);
        }

        // Un (cliente, producto) entra si tuvo ventas O devoluciones.
        var allKeys = soldDict.Keys.Union(returnedDict.Keys).ToList();

        var products = allKeys
            .Select(k => k.Item2)
            .Distinct()
            .OrderBy(pid => productNames[pid])
            .Select(pid => new ProductColumnDto(pid, productNames[pid]))
            .ToList();

        var customerIds = allKeys
            .Select(k => k.Item1)
            .Distinct()
            .OrderBy(cid => customerNames[cid])
            .ToList();

        var rows = customerIds.Select(cid =>
        {
            var sold = products.Select(p => soldDict.GetValueOrDefault((cid, p.ProductId))).ToList();
            var returned = products.Select(p => returnedDict.GetValueOrDefault((cid, p.ProductId))).ToList();
            return new CustomerProductRowDto(cid, customerNames[cid], sold, returned, sold.Sum(), returned.Sum());
        }).ToList();

        var productTotalsSold = products.Select((_, i) => rows.Sum(r => r.QuantitiesSold[i])).ToList();
        var productTotalsReturned = products.Select((_, i) => rows.Sum(r => r.QuantitiesReturned[i])).ToList();
        var grandTotalSold = productTotalsSold.Sum();
        var grandTotalReturned = productTotalsReturned.Sum();

        return new SalesByProductReportDto(
            supplierId, string.Empty, products, rows,
            productTotalsSold, productTotalsReturned, grandTotalSold, grandTotalReturned);
    }

    public async Task<UnallocatedPaymentsReportDto> GetUnallocatedPaymentsAsync()
    {
        // Total recibido por cliente (pagos activos).
        var payments = await _context.Payments
            .Where(p => p.IsActive)
            .GroupBy(p => new { p.CustomerId, CustomerName = p.Customer.Name })
            .Select(g => new {
                g.Key.CustomerId,
                g.Key.CustomerName,
                Total = g.Sum(p => p.Amount)
            })
            .ToListAsync();

        // Monto ya aplicado a remisiones por cliente (asignaciones activas de pagos activos).
        var allocated = await _context.PaymentAllocations
            .Where(a => a.IsActive && a.Payment.IsActive)
            .GroupBy(a => a.Payment.CustomerId)
            .Select(g => new { CustomerId = g.Key, Amount = g.Sum(a => a.Amount) })
            .ToDictionaryAsync(x => x.CustomerId, x => x.Amount);

        var rows = payments
            .Select(p =>
            {
                var applied = allocated.GetValueOrDefault(p.CustomerId);
                return new UnallocatedPaymentRowDto(
                    p.CustomerId, p.CustomerName, p.Total, applied, p.Total - applied);
            })
            .Where(r => r.UnallocatedAmount > 0.005m)
            .OrderBy(r => r.CustomerName)
            .ToList();

        return new UnallocatedPaymentsReportDto(rows, rows.Sum(r => r.UnallocatedAmount));
    }

    // Espejo de GetUnallocatedPaymentsAsync para el otro lado del mostrador: lo que se devolvió
    // sin decir contra qué venta. Igual que los anticipos, no se atribuye a ningún proveedor.
    public async Task<UnlinkedReturnsReportDto> GetUnlinkedReturnsAsync()
    {
        var notes = await _context.ReturnNotes
            .Where(n => n.IsActive && n.RemissionId == null)
            .Select(n => new
            {
                n.CustomerId,
                CustomerName = n.Customer.Name,
                n.Discount,
                n.UnlinkedReason,
                Subtotal = n.Details.Where(d => d.IsActive).Sum(d => d.Quantity * d.UnitPrice),
            })
            .ToListAsync();

        var rows = notes
            .GroupBy(n => new { n.CustomerId, n.CustomerName })
            .Select(g => new UnlinkedReturnRowDto(
                g.Key.CustomerId,
                g.Key.CustomerName,
                g.Count(),
                g.Sum(n => n.Subtotal - n.Discount),
                string.Join("; ", g.Select(n => n.UnlinkedReason)
                    .Where(r => !string.IsNullOrWhiteSpace(r))
                    .Distinct())))
            .OrderBy(r => r.CustomerName)
            .ToList();

        return new UnlinkedReturnsReportDto(rows, rows.Sum(r => r.UnlinkedAmount));
    }
}
