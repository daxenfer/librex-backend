using Librex.Domain.Entities;

namespace Librex.Domain.Interfaces;

public interface IReturnNoteRepository : IRepository<ReturnNote>
{
    Task<ReturnNote?> GetByIdWithDetailsAsync(int id);
    Task<IEnumerable<ReturnNote>> GetAllWithCustomerAsync();
    Task<int> GetNextFolioAsync();
}
