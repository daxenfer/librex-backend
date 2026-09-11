using Librex.Domain.Entities;
using Librex.Domain.Enums;
using Librex.Domain.Interfaces;
using Librex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Librex.Infrastructure.Repositories;

// Las cinco operaciones de IRepository<T>, escritas una sola vez. Antes cada repositorio las
// reimplementaba: UpdateAsync era las mismas cuatro líneas siete veces, y DeleteAsync las mismas
// diez (con su comentario) seis veces.
//
// Todo es virtual porque las diferencias reales entre repositorios son de consulta, no de
// escritura: qué Include lleva, con qué orden se lista, si se filtra por IsActive. Cada
// repositorio concreto sobrescribe solo lo suyo.
public abstract class Repository<T>(LibrexDbContext context) : IRepository<T> where T : BaseEntity
{
    protected readonly LibrexDbContext Context = context;

    protected DbSet<T> Set => Context.Set<T>();

    // Qué raíz del grafo de borrado representa esta entidad, o null si no arrastra dependientes.
    protected abstract DeletableEntity? DeletionRoot { get; }

    public virtual async Task<T?> GetByIdAsync(int id)
        => await Set.FirstOrDefaultAsync(e => e.Id == id && e.IsActive);

    public virtual async Task<IEnumerable<T>> GetAllAsync()
        => await Set.Where(e => e.IsActive).ToListAsync();

    public virtual async Task<T> AddAsync(T entity)
    {
        Set.Add(entity);
        await Context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        Set.Update(entity);
        await Context.SaveChangesAsync();
    }

    // Borrado lógico en cascada: la raíz y sus dependientes se marcan como inactivos en un
    // solo SaveChangesAsync. Nada se destruye, así que los documentos ya emitidos que citan
    // este registro conservan su historia intacta.
    public virtual async Task DeleteAsync(int id)
    {
        var entity = await Set.FindAsync(id);
        if (entity is null) return;

        if (DeletionRoot is { } root)
        {
            var dependents = await DeletionGraph.ResolveAsync(Context, root, id);
            dependents.Deactivate();
        }

        entity.IsActive = false;
        await Context.SaveChangesAsync();
    }
}

// Remisiones, devoluciones y pagos: lo mismo que Repository<T>, más el folio consecutivo.
public abstract class DocumentRepository<T>(LibrexDbContext context) : Repository<T>(context)
    where T : BaseEntity, IFolioEntity
{
    // No filtra IsActive a propósito: el folio de un documento eliminado queda quemado y no se
    // reutiliza, evitando colisiones con el índice único de FolioNumber.
    public async Task<int> GetNextFolioAsync()
    {
        var max = await Set.Select(d => (int?)d.FolioNumber).MaxAsync();
        return (max ?? 0) + 1;
    }
}
