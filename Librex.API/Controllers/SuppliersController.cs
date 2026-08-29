using Librex.Application.DTOs.Suppliers;
using Librex.Application.UseCases.Suppliers;
using Librex.Application.DTOs.Deletion;
using Librex.Application.UseCases.Deletion;
using Librex.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Librex.API.Controllers;

[Authorize]
[ApiController]
[Route("api/suppliers")]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _service;
    private readonly IDeletionService _deletionService;

    public SuppliersController(ISupplierService service, IDeletionService deletionService)
    {
        _service = service;
        _deletionService = deletionService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SupplierDto>>> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SupplierDto>> GetById(int id)
    {
        var supplier = await _service.GetByIdAsync(id);
        return supplier is null ? NotFound() : Ok(supplier);
    }

    [HttpPost]
    public async Task<ActionResult<SupplierDto>> Create([FromBody] CreateSupplierDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<SupplierDto>> Update(int id, [FromBody] UpdateSupplierDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return updated is null ? NotFound() : Ok(updated);
    }

    // Qué se va a borrar en cascada junto con esta entidad. Se consulta antes del DELETE
    // para que el usuario confirme con el impacto a la vista.
    [HttpGet("{id:int}/deletion-impact")]
    public async Task<ActionResult<DeletionImpactDto>> GetDeletionImpact(int id)
    {
        var impact = await _deletionService.GetImpactAsync(DeletableEntity.Supplier, id);
        return impact is null ? NotFound() : Ok(impact);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
