using Librex.Application.DTOs.Payments;

namespace Librex.Application.UseCases.Payments;

public interface IPaymentService
{
    Task<IEnumerable<PaymentDto>> GetAllAsync();
    Task<PaymentDto?> GetByIdAsync(int id);
    Task<PaymentDto> CreateAsync(CreatePaymentDto dto);
    Task<PaymentDto?> UpdateAsync(int id, UpdatePaymentDto dto);
    Task<bool> DeleteAsync(int id);
}
