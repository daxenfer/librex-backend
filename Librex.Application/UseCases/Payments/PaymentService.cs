using Librex.Application.DTOs.Payments;
using Librex.Domain.Entities;
using Librex.Domain.Interfaces;

namespace Librex.Application.UseCases.Payments;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _repository;

    public PaymentService(IPaymentRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<PaymentDto>> GetAllAsync()
        => (await _repository.GetAllWithCustomerAsync()).Select(MapToDto);

    public async Task<PaymentDto?> GetByIdAsync(int id)
    {
        var payment = await _repository.GetByIdWithCustomerAsync(id);
        return payment is null ? null : MapToDto(payment);
    }

    public async Task<PaymentDto> CreateAsync(CreatePaymentDto dto)
    {
        var folio = await _repository.GetNextFolioAsync(tenantId: 1);

        var payment = new Payment
        {
            FolioNumber = folio,
            CustomerId = dto.CustomerId,
            RemissionId = dto.RemissionId,
            Date = dto.Date,
            Amount = dto.Amount,
            PaymentMethod = dto.PaymentMethod,
            Reference = dto.Reference,
            Notes = dto.Notes,
        };

        var created = await _repository.AddAsync(payment);
        var full = await _repository.GetByIdWithCustomerAsync(created.Id);
        return MapToDto(full!);
    }

    public async Task<PaymentDto?> UpdateAsync(int id, UpdatePaymentDto dto)
    {
        var payment = await _repository.GetByIdWithCustomerAsync(id);
        if (payment is null) return null;

        payment.CustomerId = dto.CustomerId;
        payment.RemissionId = dto.RemissionId;
        payment.Date = dto.Date;
        payment.Amount = dto.Amount;
        payment.PaymentMethod = dto.PaymentMethod;
        payment.Reference = dto.Reference;
        payment.Notes = dto.Notes;
        payment.IsActive = dto.IsActive;

        await _repository.UpdateAsync(payment);
        var full = await _repository.GetByIdWithCustomerAsync(id);
        return MapToDto(full!);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var payment = await _repository.GetByIdAsync(id);
        if (payment is null) return false;
        await _repository.DeleteAsync(id);
        return true;
    }

    private static PaymentDto MapToDto(Payment p) => new()
    {
        Id = p.Id,
        FolioNumber = p.FolioNumber,
        FolioFormatted = p.FolioNumber.ToString("D6"),
        CustomerId = p.CustomerId,
        CustomerName = p.Customer?.Name ?? string.Empty,
        RemissionId = p.RemissionId,
        RemissionFolioFormatted = p.Remission.FolioNumber.ToString("D6"),
        Date = p.Date,
        Amount = p.Amount,
        PaymentMethod = p.PaymentMethod,
        Reference = p.Reference,
        Notes = p.Notes,
        IsActive = p.IsActive,
    };
}
