using Librex.Domain.Entities;
using Librex.Domain.Interfaces;
using Librex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Librex.Infrastructure.Repositories;

public class PublisherRepository : IPublisherRepository
{
    private readonly LibrexDbContext _context;

    public PublisherRepository(LibrexDbContext context)
    {
        _context = context;
    }

    public async Task<Publisher?> GetByIdAsync(int id)
        => await _context.Publishers.FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IEnumerable<Publisher>> GetAllAsync()
        => await _context.Publishers
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();

    public async Task<Publisher> AddAsync(Publisher publisher)
    {
        _context.Publishers.Add(publisher);
        await _context.SaveChangesAsync();
        return publisher;
    }

    public async Task UpdateAsync(Publisher publisher)
    {
        _context.Publishers.Update(publisher);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var publisher = await _context.Publishers.FindAsync(id);
        if (publisher is not null)
        {
            publisher.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }
}
