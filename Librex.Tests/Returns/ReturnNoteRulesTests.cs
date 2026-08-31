using System.ComponentModel.DataAnnotations;
using Librex.Application.DTOs.Payments;
using Librex.Application.DTOs.ReturnNotes;
using Librex.Application.UseCases.Payments;
using Librex.Application.UseCases.ReturnNotes;
using Librex.Domain.Exceptions;
using Librex.Infrastructure.Data;
using Librex.Infrastructure.Repositories;
using Librex.Tests.Helpers;

namespace Librex.Tests.Returns;

// Capturar sin remisión sigue permitido, pero no en silencio (hace falta un motivo) y no de
// cualquier manera: una remisión ajena al cliente nunca es aceptable.
public class ReturnNoteRulesTests : IDisposable
{
    private readonly LibrexDbContext _context = TestDbContextFactory.Create();

    public void Dispose() => _context.Dispose();

    private static List<ValidationResult> Validate(object dto)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(dto, new ValidationContext(dto), results, validateAllProperties: true);
        return results;
    }

    private static CreateReturnNoteDto NewDto(int customerId, int productId) => new()
    {
        CustomerId = customerId,
        Date = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
        Details = [new CreateReturnNoteDetailDto { ProductId = productId, Quantity = 1m, UnitPrice = 100m }],
    };

    private ReturnNoteService NewReturnNoteService()
        => new(new ReturnNoteRepository(_context), new RemissionRepository(_context));

    /* ── El motivo es obligatorio solo cuando no hay remisión ───────────── */

    [Fact]
    public void Validate_WithoutRemissionAndWithoutReason_IsInvalid()
    {
        var dto = NewDto(customerId: 1, productId: 1);

        var errors = Validate(dto);

        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(CreateReturnNoteDto.UnlinkedReason)));
    }

    [Fact]
    public void Validate_WithoutRemissionButWithReason_IsValid()
    {
        var dto = NewDto(customerId: 1, productId: 1);
        dto.UnlinkedReason = "Material de muestra que nunca se facturó";

        Assert.Empty(Validate(dto));
    }

    [Fact]
    public void Validate_WithRemissionAndWithoutReason_IsValid()
    {
        var dto = NewDto(customerId: 1, productId: 1);
        dto.RemissionId = 1;

        Assert.Empty(Validate(dto));
    }

    /* ── La remisión ligada debe ser del mismo cliente ──────────────────── */

    [Fact]
    public async Task CreateAsync_RemissionOfAnotherCustomer_Throws()
    {
        var data = await DeletionScenario.SeedAsync(_context);
        var dto = NewDto(data.Customer2.Id, data.Product1.Id);
        dto.RemissionId = data.Remission1.Id; // es del cliente 1

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(
            () => NewReturnNoteService().CreateAsync(dto));
        Assert.Contains("otro cliente", ex.Message);
    }

    [Fact]
    public async Task CreateAsync_DeletedRemission_Throws()
    {
        var data = await DeletionScenario.SeedAsync(_context);
        await new RemissionRepository(_context).DeleteAsync(data.Remission1.Id);

        var dto = NewDto(data.Customer1.Id, data.Product1.Id);
        dto.RemissionId = data.Remission1.Id;

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => NewReturnNoteService().CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_RemissionOfTheSameCustomer_Succeeds()
    {
        var data = await DeletionScenario.SeedAsync(_context);
        var dto = NewDto(data.Customer1.Id, data.Product1.Id);
        dto.RemissionId = data.Remission1.Id;

        var created = await NewReturnNoteService().CreateAsync(dto);

        Assert.Equal(data.Remission1.Id, created.RemissionId);
    }

    [Fact]
    public async Task CreateAsync_WithoutRemission_SavesTheReason()
    {
        var data = await DeletionScenario.SeedAsync(_context);
        var dto = NewDto(data.Customer1.Id, data.Product1.Id);
        dto.UnlinkedReason = "Material de muestra";

        var created = await NewReturnNoteService().CreateAsync(dto);

        Assert.Null(created.RemissionId);
        Assert.Equal("Material de muestra", created.UnlinkedReason);
    }

    /* ── Un pago solo se aplica a remisiones del mismo cliente ──────────── */

    [Fact]
    public async Task PaymentCreateAsync_AllocationToAnotherCustomersRemission_Throws()
    {
        var data = await DeletionScenario.SeedAsync(_context);
        var service = new PaymentService(new PaymentRepository(_context), new RemissionRepository(_context));

        var dto = new CreatePaymentDto
        {
            CustomerId = data.Customer2.Id,
            Date = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            Amount = 100m,
            PaymentMethod = "Efectivo",
            Allocations = [new CreatePaymentAllocationDto { RemissionId = data.Remission1.Id, Amount = 100m }],
        };

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => service.CreateAsync(dto));
        Assert.Contains("otro cliente", ex.Message);
    }

    [Fact]
    public async Task PaymentCreateAsync_AllocationToOwnRemission_Succeeds()
    {
        var data = await DeletionScenario.SeedAsync(_context);
        var service = new PaymentService(new PaymentRepository(_context), new RemissionRepository(_context));

        var dto = new CreatePaymentDto
        {
            CustomerId = data.Customer1.Id,
            Date = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            Amount = 100m,
            PaymentMethod = "Efectivo",
            Allocations = [new CreatePaymentAllocationDto { RemissionId = data.Remission1.Id, Amount = 100m }],
        };

        var created = await service.CreateAsync(dto);

        Assert.Single(created.Allocations);
    }
}
