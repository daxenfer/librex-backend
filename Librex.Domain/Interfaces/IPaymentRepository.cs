using Librex.Domain.Entities;

namespace Librex.Domain.Interfaces;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<Payment?> GetByIdWithCustomerAsync(int id);
    Task<IEnumerable<Payment>> GetAllWithCustomerAsync();
    Task<int> GetNextFolioAsync(int tenantId);
}
