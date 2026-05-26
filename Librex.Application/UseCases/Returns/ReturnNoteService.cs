using Librex.Application.DTOs.ReturnNotes;
using Librex.Domain.Entities;
using Librex.Domain.Interfaces;

namespace Librex.Application.UseCases.ReturnNotes;

public class ReturnNoteService : IReturnNoteService
{
    private readonly IReturnNoteRepository _repository;

    public ReturnNoteService(IReturnNoteRepository repository)
    {
        _repository = repository;
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
        var folio = await _repository.GetNextFolioAsync(tenantId: 1);

        var note = new ReturnNote
        {
            FolioNumber = folio,
            CustomerId = dto.CustomerId,
            RemissionId = dto.RemissionId,
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

        note.CustomerId = dto.CustomerId;
        note.RemissionId = dto.RemissionId;
        note.Date = dto.Date;
        note.Notes = dto.Notes;
        note.ReceivedBy = dto.ReceivedBy;
        note.Discount = dto.Discount;
        note.IsActive = dto.IsActive;

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

    private static ReturnNoteDto MapToDto(ReturnNote r)
    {
        var details = r.Details.Select(d => new ReturnNoteDetailDto
        {
            Id = d.Id,
            ProductId = d.ProductId,
            ProductName = d.Product?.Name ?? string.Empty,
            PublisherName = d.Product?.Publisher?.Name,
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
            RemissionFolioFormatted = r.Remission.FolioNumber.ToString("D6"),
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
