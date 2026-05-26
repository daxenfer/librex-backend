using Librex.Application.DTOs.Remissions;
using Librex.Domain.Entities;
using Librex.Domain.Interfaces;

namespace Librex.Application.UseCases.Remissions;

public class RemissionService : IRemissionService
{
    private readonly IRemissionRepository _repository;

    public RemissionService(IRemissionRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<RemissionDto>> GetAllAsync()
        => (await _repository.GetAllWithCustomerAsync()).Select(MapToDto);

    public async Task<RemissionDto?> GetByIdAsync(int id)
    {
        var remission = await _repository.GetByIdWithDetailsAsync(id);
        return remission is null ? null : MapToDto(remission);
    }

    public async Task<RemissionDto> CreateAsync(CreateRemissionDto dto)
    {
        var folio = await _repository.GetNextFolioAsync(tenantId: 1);

        var remission = new Remission
        {
            FolioNumber = folio,
            CustomerId = dto.CustomerId,
            Date = dto.Date,
            SalesPerson = dto.SalesPerson,
            Notes = dto.Notes,
            RecipientName = dto.RecipientName,
            Discount = dto.Discount,
            Details = dto.Details.Select(d => new RemissionDetail
            {
                ProductId = d.ProductId,
                City = d.City,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
            }).ToList(),
        };

        var created = await _repository.AddAsync(remission);
        var full = await _repository.GetByIdWithDetailsAsync(created.Id);
        return MapToDto(full!);
    }

    public async Task<RemissionDto?> UpdateAsync(int id, UpdateRemissionDto dto)
    {
        var remission = await _repository.GetByIdWithDetailsAsync(id);
        if (remission is null) return null;

        remission.CustomerId = dto.CustomerId;
        remission.Date = dto.Date;
        remission.SalesPerson = dto.SalesPerson;
        remission.Notes = dto.Notes;
        remission.RecipientName = dto.RecipientName;
        remission.Discount = dto.Discount;
        remission.IsActive = dto.IsActive;

        remission.Details.Clear();
        foreach (var d in dto.Details)
        {
            remission.Details.Add(new RemissionDetail
            {
                ProductId = d.ProductId,
                City = d.City,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
            });
        }

        await _repository.UpdateAsync(remission);
        var full = await _repository.GetByIdWithDetailsAsync(id);
        return MapToDto(full!);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var remission = await _repository.GetByIdAsync(id);
        if (remission is null) return false;
        await _repository.DeleteAsync(id);
        return true;
    }

    private static RemissionDto MapToDto(Remission r)
    {
        var details = r.Details.Select(d => new RemissionDetailDto
        {
            Id = d.Id,
            ProductId = d.ProductId,
            ProductName = d.Product?.Name ?? string.Empty,
            PublisherName = d.Product?.Publisher?.Name,
            City = d.City,
            Quantity = d.Quantity,
            UnitPrice = d.UnitPrice,
            Amount = d.Quantity * d.UnitPrice,
        }).ToList();

        var subtotal = details.Sum(d => d.Amount);

        return new RemissionDto
        {
            Id = r.Id,
            FolioNumber = r.FolioNumber,
            FolioFormatted = r.FolioNumber.ToString("D6"),
            CustomerId = r.CustomerId,
            CustomerName = r.Customer?.Name ?? string.Empty,
            Date = r.Date,
            SalesPerson = r.SalesPerson,
            Notes = r.Notes,
            RecipientName = r.RecipientName,
            Discount = r.Discount,
            Subtotal = subtotal,
            Total = subtotal - r.Discount,
            IsActive = r.IsActive,
            Details = details,
        };
    }
}
