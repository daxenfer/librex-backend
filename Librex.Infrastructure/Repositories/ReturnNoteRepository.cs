using Librex.Domain.Entities;
using Librex.Domain.Enums;
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
                    .ThenInclude(p => p.Supplier)
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

    public async Task<int> GetNextFolioAsync()
    {
        var max = await _context.ReturnNotes
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

    // Borrado físico en cascada. Un solo SaveChangesAsync para que EF lo resuelva
    // dentro de una transacción: o se va todo, o no se va nada.
    public async Task DeleteAsync(int id)
    {
        var returnNote = await _context.ReturnNotes.FindAsync(id);
        if (returnNote is null) return;

        var dependents = await DeletionGraph.ResolveAsync(_context, DeletableEntity.ReturnNote, id);
        dependents.RemoveFrom(_context);
        _context.ReturnNotes.Remove(returnNote);
        await _context.SaveChangesAsync();
    }
}
