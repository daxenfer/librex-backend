using Librex.Application.DTOs.ReturnNotes;
using Librex.Domain.Entities;
using Librex.Domain.Exceptions;
using Librex.Domain.Interfaces;

namespace Librex.Application.UseCases.ReturnNotes;

public class ReturnNoteService : IReturnNoteService
{
    private readonly IReturnNoteRepository _repository;
    private readonly IRemissionRepository _remissions;

    public ReturnNoteService(IReturnNoteRepository repository, IRemissionRepository remissions)
    {
        _repository = repository;
        _remissions = remissions;
    }

    public async Task<IEnumerable<ReturnNoteDto>> GetAllAsync()
        => (await _repository.GetAllWithCustomerAsync()).Select(MapToDto);

    public async Task<ReturnNoteDto?> GetByIdAsync(int id)
    {
        var note = await _repository.GetByIdWithDetailsAsync(id);
        return note is null ? null : MapToDto(note);
    }

    public async Task<ReturnNoteDto> CreateAsync(CreateReturnNoteDto dto)
    {
        await EnsureRemissionBelongsToCustomerAsync(dto.RemissionId, dto.CustomerId);

        var folio = await _repository.GetNextFolioAsync();

        var note = new ReturnNote
        {
            FolioNumber = folio,
            CustomerId = dto.CustomerId,
            RemissionId = dto.RemissionId,
            UnlinkedReason = dto.UnlinkedReason,
            Date = dto.Date,
            Notes = dto.Notes,
            ReceivedBy = dto.ReceivedBy,
            Discount = dto.Discount,
            Details = dto.Details.Select(d => new ReturnNoteDetail
            {
                ProductId = d.ProductId,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
            }).ToList(),
        };

        var created = await _repository.AddAsync(note);
        var full = await _repository.GetByIdWithDetailsAsync(created.Id);
        return MapToDto(full!);
    }

    public async Task<ReturnNoteDto?> UpdateAsync(int id, UpdateReturnNoteDto dto)
    {
        var note = await _repository.GetByIdWithDetailsAsync(id);
        if (note is null) return null;

        await EnsureRemissionBelongsToCustomerAsync(dto.RemissionId, dto.CustomerId);

        note.CustomerId = dto.CustomerId;
        note.RemissionId = dto.RemissionId;
        note.UnlinkedReason = dto.UnlinkedReason;
        note.Date = dto.Date;
        note.Notes = dto.Notes;
        note.ReceivedBy = dto.ReceivedBy;
        note.Discount = dto.Discount;

        note.Details.Clear();
        foreach (var d in dto.Details)
        {
            note.Details.Add(new ReturnNoteDetail
            {
                ProductId = d.ProductId,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
            });
        }

        await _repository.UpdateAsync(note);
        var full = await _repository.GetByIdWithDetailsAsync(id);
        return MapToDto(full!);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var note = await _repository.GetByIdAsync(id);
        if (note is null) return false;
        await _repository.DeleteAsync(id);
        return true;
    }

    // Una devolución solo puede colgar de una remisión del mismo cliente. El combo del formulario
    // ya filtra, pero por API no había nada que lo impidiera.
    private async Task EnsureRemissionBelongsToCustomerAsync(int? remissionId, int customerId)
    {
        if (remissionId is null) return;

        var remission = await _remissions.GetByIdAsync(remissionId.Value);
        if (remission is null)
            throw new BusinessRuleException("La remisión indicada no existe o fue eliminada.");
        if (remission.CustomerId != customerId)
            throw new BusinessRuleException("La remisión indicada pertenece a otro cliente.");
    }

    private static ReturnNoteDto MapToDto(ReturnNote r)
    {
        var details = r.Details.Select(d => new ReturnNoteDetailDto
        {
            Id = d.Id,
            ProductId = d.ProductId,
            ProductName = d.Product?.Name ?? string.Empty,
            SupplierName = d.Product?.Supplier?.Name,
            Quantity = d.Quantity,
            UnitPrice = d.UnitPrice,
            Amount = d.Quantity * d.UnitPrice,
        }).ToList();

        var subtotal = details.Sum(d => d.Amount);

        return new ReturnNoteDto
        {
            Id = r.Id,
            FolioNumber = r.FolioNumber,
            FolioFormatted = r.FolioNumber.ToString("D6"),
            CustomerId = r.CustomerId,
            CustomerName = r.Customer?.Name ?? string.Empty,
            RemissionId = r.RemissionId,
            RemissionFolioFormatted = r.Remission?.FolioNumber.ToString("D6") ?? string.Empty,
            UnlinkedReason = r.UnlinkedReason,
            Date = r.Date,
            Notes = r.Notes,
            ReceivedBy = r.ReceivedBy,
            Discount = r.Discount,
            Subtotal = subtotal,
            Total = subtotal - r.Discount,
            IsActive = r.IsActive,
            Details = details,
        };
    }
}
