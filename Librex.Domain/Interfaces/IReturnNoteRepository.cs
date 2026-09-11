using Librex.Domain.Entities;

namespace Librex.Domain.Interfaces;

public interface IReturnNoteRepository : IRepository<ReturnNote>
{
    Task<ReturnNote?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<ReturnNote>> GetAllWithCustomerAsync(CancellationToken ct = default);
    Task<int> GetNextFolioAsync(CancellationToken ct = default);
}
