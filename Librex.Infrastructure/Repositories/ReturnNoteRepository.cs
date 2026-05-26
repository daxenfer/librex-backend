using Librex.Domain.Entities;
using Librex.Domain.Interfaces;
using Librex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Librex.Infrastructure.Repositories;

public class ReturnNoteRepository : IReturnNoteRepository
{
    private readonly LibrexDbContext _context;

    public ReturnNoteRepository(LibrexDbContext context)
    {
        _context = context;
    }

    public async Task<ReturnNote?> GetByIdAsync(int id)
        => await _context.ReturnNotes.FirstOrDefaultAsync(r => r.Id == id);

    public async Task<ReturnNote?> GetByIdWithDetailsAsync(int id)
        => await _context.ReturnNotes
            .Include(r => r.Customer)
            .Include(r => r.Remission)
            .Include(r => r.Details)
                .ThenInclude(d => d.Product)
                    .ThenInclude(p => p.Publisher)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<IEnumerable<ReturnNote>> GetAllAsync()
        => await _context.ReturnNotes
            .Where(r => r.IsActive)
            .OrderByDescending(r => r.Date)
            .ToListAsync();

    public async Task<IEnumerable<ReturnNote>> GetAllWithCustomerAsync()
        => await _context.ReturnNotes
            .Include(r => r.Customer)
            .Include(r => r.Remission)
            .Include(r => r.Details)
                .ThenInclude(d => d.Product)
            .Where(r => r.IsActive)
            .OrderByDescending(r => r.Date)
            .ToListAsync();

    public async Task<int> GetNextFolioAsync(int tenantId)
    {
        var max = await _context.ReturnNotes
            .Where(r => r.TenantId == tenantId)
            .Select(r => (int?)r.FolioNumber)
            .MaxAsync();
        return (max ?? 0) + 1;
    }

    public async Task<ReturnNote> AddAsync(ReturnNote returnNote)
    {
        _context.ReturnNotes.Add(returnNote);
        await _context.SaveChangesAsync();
        return returnNote;
    }

    public async Task UpdateAsync(ReturnNote returnNote)
    {
        _context.ReturnNotes.Update(returnNote);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var returnNote = await _context.ReturnNotes.FindAsync(id);
        if (returnNote is not null)
        {
            returnNote.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }
}
