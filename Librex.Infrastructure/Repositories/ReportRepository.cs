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

    public async Task<PublisherReportDto> GetByPublisherAsync(int? publisherId, int tenantId)
    {
        var salesQuery = _context.RemissionDetails
            .Where(d => d.IsActive && d.Remission.IsActive && d.Remission.TenantId == tenantId);
        if (publisherId.HasValue)
            salesQuery = salesQuery.Where(d => d.Product.PublisherId == publisherId.Value);

        var sales = await salesQuery
            .GroupBy(d => new { d.Remission.CustomerId, Name = d.Remission.Customer.Name })
            .Select(g => new { g.Key.CustomerId, g.Key.Name, Total = g.Sum(d => d.Quantity * d.UnitPrice) })
            .ToListAsync();

        var returnsQuery = _context.ReturnNoteDetails
            .Where(d => d.IsActive && d.ReturnNote.IsActive && d.ReturnNote.TenantId == tenantId);
        if (publisherId.HasValue)
            returnsQuery = returnsQuery.Where(d => d.Product.PublisherId == publisherId.Value);

        var returns = await returnsQuery
            .GroupBy(d => new { d.ReturnNote.CustomerId, Name = d.ReturnNote.Customer.Name })
            .Select(g => new { g.Key.CustomerId, g.Key.Name, Total = g.Sum(d => d.Quantity * d.UnitPrice) })
            .ToListAsync();

        var payments = await _context.Payments
            .Where(p => p.IsActive && p.TenantId == tenantId)
            .GroupBy(p => new { p.CustomerId, Name = p.Customer.Name })
            .Select(g => new { g.Key.CustomerId, g.Key.Name, Total = g.Sum(p => p.Amount) })
            .ToListAsync();

        var allCustomerIds = sales.Select(s => s.CustomerId)
            .Union(returns.Select(r => r.CustomerId))
            .Union(payments.Select(p => p.CustomerId))
            .Distinct();

        var salesDict = sales.ToDictionary(s => s.CustomerId, s => (s.Name, s.Total));
        var returnsDict = returns.ToDictionary(r => r.CustomerId, r => r.Total);
        var paymentsDict = payments.ToDictionary(p => p.CustomerId, p => (p.Name, p.Total));

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

        return new PublisherReportDto(publisherId, string.Empty, rows, totals);
    }

    public async Task<SalesByProductReportDto> GetSalesByProductAsync(int? publisherId, int tenantId)
    {
        var salesQuery = _context.RemissionDetails
            .Where(d => d.IsActive && d.Remission.IsActive && d.Remission.TenantId == tenantId);
        if (publisherId.HasValue)
            salesQuery = salesQuery.Where(d => d.Product.PublisherId == publisherId.Value);

        var sales = await salesQuery
            .GroupBy(d => new { d.Remission.CustomerId, CustomerName = d.Remission.Customer.Name,
                                d.ProductId, ProductName = d.Product.Name })
            .Select(g => new { g.Key.CustomerId, g.Key.CustomerName,
                               g.Key.ProductId, g.Key.ProductName, Qty = (int)g.Sum(d => d.Quantity) })
            .ToListAsync();

        var returnsQuery = _context.ReturnNoteDetails
            .Where(d => d.IsActive && d.ReturnNote.IsActive && d.ReturnNote.TenantId == tenantId);
        if (publisherId.HasValue)
            returnsQuery = returnsQuery.Where(d => d.Product.PublisherId == publisherId.Value);

        var returns = await returnsQuery
            .GroupBy(d => new { d.ReturnNote.CustomerId, CustomerName = d.ReturnNote.Customer.Name,
                                d.ProductId, ProductName = d.Product.Name })
            .Select(g => new { g.Key.CustomerId, g.Key.CustomerName,
                               g.Key.ProductId, g.Key.ProductName, Qty = (int)g.Sum(d => d.Quantity) })
            .ToListAsync();

        // net quantity per (customerId, productId)
        var netDict = new Dictionary<(int, int), int>();
        var customerNames = new Dictionary<int, string>();
        var productNames = new Dictionary<int, string>();

        foreach (var s in sales)
        {
            var key = (s.CustomerId, s.ProductId);
            netDict[key] = netDict.GetValueOrDefault(key) + s.Qty;
            customerNames.TryAdd(s.CustomerId, s.CustomerName);
            productNames.TryAdd(s.ProductId, s.ProductName);
        }
        foreach (var r in returns)
        {
            var key = (r.CustomerId, r.ProductId);
            netDict[key] = netDict.GetValueOrDefault(key) - r.Qty;
            customerNames.TryAdd(r.CustomerId, r.CustomerName);
            productNames.TryAdd(r.ProductId, r.ProductName);
        }

        // keep only positive net quantities
        var positiveEntries = netDict.Where(kv => kv.Value > 0).ToList();

        var products = positiveEntries
            .Select(kv => kv.Key.Item2)
            .Distinct()
            .OrderBy(pid => productNames[pid])
            .Select(pid => new ProductColumnDto(pid, productNames[pid]))
            .ToList();

        var customerIds = positiveEntries
            .Select(kv => kv.Key.Item1)
            .Distinct()
            .OrderBy(cid => customerNames[cid])
            .ToList();

        var rows = customerIds.Select(cid =>
        {
            var quantities = products.Select(p => netDict.GetValueOrDefault((cid, p.ProductId))).ToList();
            return new CustomerProductRowDto(cid, customerNames[cid], quantities, quantities.Sum());
        }).ToList();

        var productTotals = products.Select(p => rows.Sum(r => r.Quantities[products.IndexOf(p)])).ToList();
        var grandTotal = productTotals.Sum();

        return new SalesByProductReportDto(publisherId, string.Empty, products, rows, productTotals, grandTotal);
    }
}
