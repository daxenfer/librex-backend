using Librex.Application.DTOs.Suppliers;
using Librex.Application.UseCases.Suppliers;
using Librex.Application.DTOs.Deletion;
using Librex.Application.UseCases.Deletion;
using Librex.Domain.Constants;
using Librex.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Librex.API.Controllers;

[Authorize]
[ApiController]
[Route("api/suppliers")]
public sealed class SuppliersController(ISupplierService service, IDeletionService deletionService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SupplierDto>>> GetAll()
        => Ok(await service.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SupplierDto>> GetById(int id)
    {
        var supplier = await service.GetByIdAsync(id);
        return supplier is null ? NotFound() : Ok(supplier);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.SuppliersWrite)]
    public async Task<ActionResult<SupplierDto>> Create([FromBody] CreateSupplierDto dto)
    {
        var created = await service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = Permissions.SuppliersWrite)]
    public async Task<ActionResult<SupplierDto>> Update(int id, [FromBody] UpdateSupplierDto dto)
    {
        var updated = await service.UpdateAsync(id, dto);
        return updated is null ? NotFound() : Ok(updated);
    }

    // Qué se va a borrar en cascada junto con esta entidad. Se consulta antes del DELETE
    // para que el usuario confirme con el impacto a la vista.
    [HttpGet("{id:int}/deletion-impact")]
    public async Task<ActionResult<DeletionImpactDto>> GetDeletionImpact(int id)
    {
        var impact = await deletionService.GetImpactAsync(DeletableEntity.Supplier, id);
        return impact is null ? NotFound() : Ok(impact);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Permissions.SuppliersDelete)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
