using Librex.Application.DTOs.Payments;
using Librex.Domain.Entities;
using Librex.Domain.Exceptions;
using Librex.Domain.Interfaces;

namespace Librex.Application.UseCases.Payments;

public sealed class PaymentService(IPaymentRepository repository, IRemissionRepository remissions) : IPaymentService
{
    public async Task<IEnumerable<PaymentDto>> GetAllAsync(CancellationToken ct = default)
        => (await repository.GetAllWithCustomerAsync(ct)).Select(MapToDto);

    public async Task<PaymentDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var payment = await repository.GetByIdWithCustomerAsync(id, ct);
        return payment is null ? null : MapToDto(payment);
    }

    public async Task<PaymentDto> CreateAsync(CreatePaymentDto dto, CancellationToken ct = default)
    {
        await EnsureAllocationsBelongToCustomerAsync(dto.Allocations, dto.CustomerId, ct);

        var folio = await repository.GetNextFolioAsync(ct);

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

        var created = await repository.AddAsync(payment, ct);
        var full = await repository.GetByIdWithCustomerAsync(created.Id, ct);
        return MapToDto(full!);
    }

    public async Task<PaymentDto?> UpdateAsync(int id, UpdatePaymentDto dto, CancellationToken ct = default)
    {
        var payment = await repository.GetByIdWithCustomerAsync(id, ct);
        if (payment is null) return null;

        await EnsureAllocationsBelongToCustomerAsync(dto.Allocations, dto.CustomerId, ct);

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

        payment.Allocations.Clear();
        foreach (var a in BuildAllocations(dto.Allocations))
            payment.Allocations.Add(a);

        await repository.UpdateAsync(payment, ct);
        var full = await repository.GetByIdWithCustomerAsync(id, ct);
        return MapToDto(full!);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var payment = await repository.GetByIdAsync(id, ct);
        if (payment is null) return false;
        await repository.DeleteAsync(id, ct);
        return true;
    }

    // Un pago solo puede aplicarse a remisiones del mismo cliente. El editor de reparto ya solo
    // ofrece las suyas, pero por API no había nada que lo impidiera.
    private async Task EnsureAllocationsBelongToCustomerAsync(
        IEnumerable<CreatePaymentAllocationDto> allocations, int customerId, CancellationToken ct = default)
    {
        var remissionIds = allocations.Where(a => a.Amount > 0).Select(a => a.RemissionId).Distinct();
        foreach (var remissionId in remissionIds)
        {
            var remission = await remissions.GetByIdAsync(remissionId, ct);
            if (remission is null)
                throw new BusinessRuleException("Una de las remisiones del reparto no existe o fue eliminada.");
            if (remission.CustomerId != customerId)
                throw new BusinessRuleException($"La remisión {remission.FolioNumber:D6} pertenece a otro cliente.");
        }
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
            RemissionFolioFormatted = Folio.Format(a.Remission?.FolioNumber),
            Amount = a.Amount,
        }).ToList();

        var applied = allocations.Sum(a => a.Amount);

        return new PaymentDto
        {
            Id = p.Id,
            FolioNumber = p.FolioNumber,
            FolioFormatted = Folio.Format(p.FolioNumber),
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
