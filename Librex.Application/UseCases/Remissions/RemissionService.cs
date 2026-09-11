using Librex.Application.DTOs.Remissions;
using Librex.Domain.Entities;
using Librex.Domain.Interfaces;

namespace Librex.Application.UseCases.Remissions;

public sealed class RemissionService(IRemissionRepository repository) : IRemissionService
{
    public async Task<IEnumerable<RemissionDto>> GetAllAsync(CancellationToken ct = default)
        => (await repository.GetAllWithCustomerAsync(ct)).Select(MapToDto);

    public async Task<RemissionDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var remission = await repository.GetByIdWithDetailsAsync(id, ct);
        return remission is null ? null : MapToDto(remission);
    }

    public async Task<RemissionDto> CreateAsync(CreateRemissionDto dto, CancellationToken ct = default)
    {
        var folio = await repository.GetNextFolioAsync(ct);

        var remission = new Remission
        {
            FolioNumber = folio,
            CustomerId = dto.CustomerId,
            // Postgres guarda la columna como timestamptz y Npgsql exige Kind=Utc; se re-etiqueta
            // la hora local (sin convertirla) para que se guarde el valor de reloj tal cual, no UTC.
            //
            // PENDIENTE: "la hora local" es la del servidor, no la del negocio. En la máquina de
            // desarrollo eso es Monterrey y sale bien; desplegado en un servidor que corra en UTC,
            // la fecha de la remisión queda seis horas adelantada. Arreglarlo es decidir de dónde
            // sale la fecha —del cliente, o fijada a America/Monterrey— y eso cambia el
            // comportamiento del negocio, así que no entra en la migración. Por eso este servicio
            // todavía no recibe TimeProvider: no tiene caso inyectar el reloj sin resolver antes
            // cuál es la zona correcta.
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

        var created = await repository.AddAsync(remission, ct);
        var full = await repository.GetByIdWithDetailsAsync(created.Id, ct);
        return MapToDto(full!);
    }

    public async Task<RemissionDto?> UpdateAsync(int id, UpdateRemissionDto dto, CancellationToken ct = default)
    {
        var remission = await repository.GetByIdWithDetailsAsync(id, ct);
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

        await repository.UpdateAsync(remission, ct);
        var full = await repository.GetByIdWithDetailsAsync(id, ct);
        return MapToDto(full!);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var remission = await repository.GetByIdAsync(id, ct);
        if (remission is null) return false;
        await repository.DeleteAsync(id, ct);
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
