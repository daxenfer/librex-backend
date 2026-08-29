using Librex.Domain.Enums;
using Librex.Domain.Interfaces;
using Librex.Infrastructure.Data;

namespace Librex.Infrastructure.Repositories;

public class DeletionRepository : IDeletionRepository
{
    private readonly LibrexDbContext _context;

    public DeletionRepository(LibrexDbContext context)
    {
        _context = context;
    }

    public async Task<DeletionImpact?> GetImpactAsync(DeletableEntity entity, int id)
    {
        var label = await GetLabelAsync(entity, id);
        if (label is null) return null;

        var dependents = await DeletionGraph.ResolveAsync(_context, entity, id);
        return new DeletionImpact(entity, id, label, dependents.ToDependents());
    }

    private async Task<string?> GetLabelAsync(DeletableEntity entity, int id) => entity switch
    {
        DeletableEntity.Customer => (await _context.Customers.FindAsync(id))?.Name,
        DeletableEntity.Supplier => (await _context.Suppliers.FindAsync(id))?.Name,
        DeletableEntity.Product => (await _context.Products.FindAsync(id))?.Name,
        DeletableEntity.Remission => Folio((await _context.Remissions.FindAsync(id))?.FolioNumber),
        DeletableEntity.ReturnNote => Folio((await _context.ReturnNotes.FindAsync(id))?.FolioNumber),
        DeletableEntity.Payment => Folio((await _context.Payments.FindAsync(id))?.FolioNumber),
        _ => null,
    };

    private static string? Folio(int? folioNumber) => folioNumber is null ? null : $"Folio {folioNumber}";
}
