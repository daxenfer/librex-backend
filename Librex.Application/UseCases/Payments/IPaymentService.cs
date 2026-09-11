using Librex.Application.DTOs.Payments;

namespace Librex.Application.UseCases.Payments;

public interface IPaymentService
{
    Task<IEnumerable<PaymentDto>> GetAllAsync(CancellationToken ct = default);
    Task<PaymentDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PaymentDto> CreateAsync(CreatePaymentDto dto, CancellationToken ct = default);
    Task<PaymentDto?> UpdateAsync(int id, UpdatePaymentDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
