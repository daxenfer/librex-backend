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
        var folio = await _repository.GetNextFolioAsync();

        var payment = new Payment
        {
            FolioNumber = folio,
            CustomerId = dto.CustomerId,
            Date = dto.Date,
            Amount = dto.Amount,
            PaymentMethod = dto.PaymentMethod,
            Reference = dto.Reference,
            Notes = dto.Notes,
            ReceivedFrom = dto.ReceivedFrom,
            Concept = dto.Concept,
            CollectedBy = dto.CollectedBy,
            City = dto.City,
            Allocations = BuildAllocations(dto.Allocations),
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
        payment.Date = dto.Date;
        payment.Amount = dto.Amount;
        payment.PaymentMethod = dto.PaymentMethod;
        payment.Reference = dto.Reference;
        payment.Notes = dto.Notes;
        payment.ReceivedFrom = dto.ReceivedFrom;
        payment.Concept = dto.Concept;
        payment.CollectedBy = dto.CollectedBy;
        payment.City = dto.City;
        payment.IsActive = dto.IsActive;

        payment.Allocations.Clear();
        foreach (var a in BuildAllocations(dto.Allocations))
            payment.Allocations.Add(a);

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

    // Solo asignaciones con monto positivo; la suma no debe exceder el monto recibido
    // (el remanente queda como anticipo a favor del cliente).
    private static List<PaymentAllocation> BuildAllocations(IEnumerable<CreatePaymentAllocationDto> allocations)
        => allocations
            .Where(a => a.Amount > 0)
            .Select(a => new PaymentAllocation
            {
                RemissionId = a.RemissionId,
                Amount = a.Amount,
            })
            .ToList();

    private static PaymentDto MapToDto(Payment p)
    {
        var allocations = p.Allocations.Select(a => new PaymentAllocationDto
        {
            RemissionId = a.RemissionId,
            RemissionFolioFormatted = a.Remission?.FolioNumber.ToString("D6") ?? string.Empty,
            Amount = a.Amount,
        }).ToList();

        var applied = allocations.Sum(a => a.Amount);

        return new PaymentDto
        {
            Id = p.Id,
            FolioNumber = p.FolioNumber,
            FolioFormatted = p.FolioNumber.ToString("D6"),
            CustomerId = p.CustomerId,
            CustomerName = p.Customer?.Name ?? string.Empty,
            Date = p.Date,
            Amount = p.Amount,
            AppliedAmount = applied,
            UnappliedAmount = p.Amount - applied,
            PaymentMethod = p.PaymentMethod,
            Reference = p.Reference,
            Notes = p.Notes,
            ReceivedFrom = p.ReceivedFrom,
            Concept = p.Concept,
            CollectedBy = p.CollectedBy,
            City = p.City,
            IsActive = p.IsActive,
            Allocations = allocations,
        };
    }
}
