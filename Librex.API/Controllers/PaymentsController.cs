using Librex.Application.DTOs.Payments;
using Librex.Application.UseCases.Payments;
using Librex.Application.DTOs.Deletion;
using Librex.Application.UseCases.Deletion;
using Librex.Domain.Constants;
using Librex.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Librex.API.Controllers;

[Authorize]
[ApiController]
[Route("api/payments")]
public sealed class PaymentsController(IPaymentService service, IDeletionService deletionService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetAll()
        => Ok(await service.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PaymentDto>> GetById(int id)
    {
        var payment = await service.GetByIdAsync(id);
        return payment is null ? NotFound() : Ok(payment);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.PaymentsWrite)]
    public async Task<ActionResult<PaymentDto>> Create([FromBody] CreatePaymentDto dto)
    {
        var created = await service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = Permissions.PaymentsWrite)]
    public async Task<ActionResult<PaymentDto>> Update(int id, [FromBody] UpdatePaymentDto dto)
    {
        var updated = await service.UpdateAsync(id, dto);
        return updated is null ? NotFound() : Ok(updated);
    }

    // Qué se va a borrar en cascada junto con esta entidad. Se consulta antes del DELETE
    // para que el usuario confirme con el impacto a la vista.
    [HttpGet("{id:int}/deletion-impact")]
    public async Task<ActionResult<DeletionImpactDto>> GetDeletionImpact(int id)
    {
        var impact = await deletionService.GetImpactAsync(DeletableEntity.Payment, id);
        return impact is null ? NotFound() : Ok(impact);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Permissions.PaymentsDelete)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
