using Librex.Domain.Entities;
using Librex.Domain.Enums;
using Librex.Domain.Interfaces;
using Librex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Librex.Infrastructure.Repositories;

public class RemissionRepository : IRemissionRepository
{
    private readonly LibrexDbContext _context;

    public RemissionRepository(LibrexDbContext context)
    {
        _context = context;
    }

    public async Task<Remission?> GetByIdAsync(int id)
        => await _context.Remissions.FirstOrDefaultAsync(r => r.Id == id && r.IsActive);

    // Details no se filtra por IsActive a propósito: un renglón solo se da de baja cuando su
    // remisión también, y esta ya viene filtrada. Al editar, los renglones se reemplazan
    // físicamente, así que nunca hay filas inactivas colgando de una remisión activa.
    public async Task<Remission?> GetByIdWithDetailsAsync(int id)
        => await _context.Remissions
            .Include(r => r.Customer)
            .Include(r => r.Details)
                .ThenInclude(d => d.Product)
                    .ThenInclude(p => p.Supplier)
            .FirstOrDefaultAsync(r => r.Id == id && r.IsActive);

    public async Task<IEnumerable<Remission>> GetAllAsync()
        => await _context.Remissions
            .Where(r => r.IsActive)
            .OrderByDescending(r => r.Date)
            .ToListAsync();

    public async Task<IEnumerable<Remission>> GetAllWithCustomerAsync()
        => await _context.Remissions
            .Include(r => r.Customer)
            .Include(r => r.Details)
            .Where(r => r.IsActive)
            .OrderByDescending(r => r.Date)
            .ToListAsync();

    // No filtra IsActive a propósito: el folio de un documento eliminado queda quemado y no se
    // reutiliza, evitando colisiones con el índice único de FolioNumber.
    public async Task<int> GetNextFolioAsync()
    {
        var max = await _context.Remissions
            .Select(r => (int?)r.FolioNumber)
            .MaxAsync();
        return (max ?? 0) + 1;
    }

    public async Task<Remission> AddAsync(Remission remission)
    {
        _context.Remissions.Add(remission);
        await _context.SaveChangesAsync();
        return remission;
    }

    public async Task UpdateAsync(Remission remission)
    {
        _context.Remissions.Update(remission);
        await _context.SaveChangesAsync();
    }

    // Borrado lógico en cascada: la raíz y sus dependientes se marcan como inactivos en un
    // solo SaveChangesAsync. Nada se destruye, así que los documentos ya emitidos que citan
    // este registro conservan su historia intacta.
    public async Task DeleteAsync(int id)
    {
        var remission = await _context.Remissions.FindAsync(id);
        if (remission is null) return;

        var dependents = await DeletionGraph.ResolveAsync(_context, DeletableEntity.Remission, id);
        dependents.Deactivate();
        remission.IsActive = false;
        await _context.SaveChangesAsync();
    }
}
