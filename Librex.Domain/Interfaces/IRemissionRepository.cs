using Librex.Domain.Entities;

namespace Librex.Domain.Interfaces;

public interface IRemissionRepository : IRepository<Remission>
{
    Task<Remission?> GetByIdWithDetailsAsync(int id);
    Task<IEnumerable<Remission>> GetAllWithCustomerAsync();
    Task<int> GetNextFolioAsync(int tenantId);
}
