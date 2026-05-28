using Librex.Domain.Entities;
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
        => await _context.Remissions.FirstOrDefaultAsync(r => r.Id == id);

    public async Task<Remission?> GetByIdWithDetailsAsync(int id)
        => await _context.Remissions
            .Include(r => r.Customer)
            .Include(r => r.Details)
                .ThenInclude(d => d.Product)
                    .ThenInclude(p => p.Publisher)
            .FirstOrDefaultAsync(r => r.Id == id);

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

    public async Task DeleteAsync(int id)
    {
        var remission = await _context.Remissions.FindAsync(id);
        if (remission is not null)
        {
            remission.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }
}
