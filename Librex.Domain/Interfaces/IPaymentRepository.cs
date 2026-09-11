using Librex.Domain.Entities;

namespace Librex.Domain.Interfaces;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<Payment?> GetByIdWithCustomerAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Payment>> GetAllWithCustomerAsync(CancellationToken ct = default);
    Task<int> GetNextFolioAsync(CancellationToken ct = default);
}
