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
        var folio = await _repository.GetNextFolioAsync();

        var remission = new Remission
        {
            FolioNumber = folio,
            CustomerId = dto.CustomerId,
            // Postgres guarda la columna como timestamptz y Npgsql exige Kind=Utc; se re-etiqueta
            // la hora local (sin convertirla) para que se guarde el valor de reloj tal cual, no UTC.
            Date = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
            SalesPerson = dto.SalesPerson,
            Notes = dto.Notes,
            RecipientName = dto.RecipientName,
            PurchaseOrder = dto.PurchaseOrder,
            Discount = dto.DiscountAmount,
            DeliveryDate = dto.DeliveryDate,
            PaymentDueDate = dto.PaymentDueDate,
            ReturnPercentage = dto.ReturnPercentage,
            ReturnDueDate = dto.ReturnDueDate,
            Details = dto.Details.Select(d => new RemissionDetail
            {
                ProductId = d.ProductId,
                Teacher = d.Teacher,
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
        remission.SalesPerson = dto.SalesPerson;
        remission.Notes = dto.Notes;
        remission.RecipientName = dto.RecipientName;
        remission.PurchaseOrder = dto.PurchaseOrder;
        remission.Discount = dto.DiscountAmount;
        remission.DeliveryDate = dto.DeliveryDate;
        remission.PaymentDueDate = dto.PaymentDueDate;
        remission.ReturnPercentage = dto.ReturnPercentage;
        remission.ReturnDueDate = dto.ReturnDueDate;

        remission.Details.Clear();
        foreach (var d in dto.Details)
        {
            remission.Details.Add(new RemissionDetail
            {
                ProductId = d.ProductId,
                Teacher = d.Teacher,
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
            Isbn = d.Product?.Isbn,
            SupplierName = d.Product?.Supplier?.Name,
            Teacher = d.Teacher,
            Quantity = d.Quantity,
            UnitPrice = d.UnitPrice,
            Amount = d.Quantity * d.UnitPrice,
        }).ToList();

        var subtotal = details.Sum(d => d.Amount);
        var discountAmount = r.Discount;

        return new RemissionDto
        {
            Id = r.Id,
            FolioNumber = r.FolioNumber,
            FolioFormatted = r.FolioNumber.ToString("D6"),
            CustomerId = r.CustomerId,
            CustomerName = r.Customer?.Name ?? string.Empty,
            CustomerAddress = r.Customer?.Address ?? string.Empty,
            CustomerPostalCode = r.Customer?.PostalCode ?? string.Empty,
            CustomerPhone = r.Customer?.Phone ?? string.Empty,
            CustomerCity = r.Customer?.City ?? string.Empty,
            Date = r.Date,
            CreatedAt = r.CreatedAt,
            SalesPerson = r.SalesPerson,
            Notes = r.Notes,
            RecipientName = r.RecipientName,
            PurchaseOrder = r.PurchaseOrder,
            DeliveryDate = r.DeliveryDate,
            PaymentDueDate = r.PaymentDueDate,
            ReturnPercentage = r.ReturnPercentage,
            ReturnDueDate = r.ReturnDueDate,
            DiscountAmount = discountAmount,
            Subtotal = subtotal,
            Total = subtotal - discountAmount,
            IsActive = r.IsActive,
            Details = details,
        };
    }
}
