using Librex.Domain.Entities;

namespace Librex.Domain.Interfaces;

public interface IRemissionRepository : IRepository<Remission>
{
    Task<Remission?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Remission>> GetAllWithCustomerAsync(CancellationToken ct = default);
    Task<int> GetNextFolioAsync(CancellationToken ct = default);
}
