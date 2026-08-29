using Librex.Application.DTOs.Remissions;
using Librex.Application.UseCases.Remissions;
using Librex.Application.DTOs.Deletion;
using Librex.Application.UseCases.Deletion;
using Librex.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Librex.API.Controllers;

[Authorize]
[ApiController]
[Route("api/remissions")]
public class RemissionsController : ControllerBase
{
    private readonly IRemissionService _service;
    private readonly IDeletionService _deletionService;

    public RemissionsController(IRemissionService service, IDeletionService deletionService)
    {
        _service = service;
        _deletionService = deletionService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RemissionDto>>> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RemissionDto>> GetById(int id)
    {
        var remission = await _service.GetByIdAsync(id);
        return remission is null ? NotFound() : Ok(remission);
    }

    [HttpPost]
    public async Task<ActionResult<RemissionDto>> Create([FromBody] CreateRemissionDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<RemissionDto>> Update(int id, [FromBody] UpdateRemissionDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return updated is null ? NotFound() : Ok(updated);
    }

    // Qué se va a borrar en cascada junto con esta entidad. Se consulta antes del DELETE
    // para que el usuario confirme con el impacto a la vista.
    [HttpGet("{id:int}/deletion-impact")]
    public async Task<ActionResult<DeletionImpactDto>> GetDeletionImpact(int id)
    {
        var impact = await _deletionService.GetImpactAsync(DeletableEntity.Remission, id);
        return impact is null ? NotFound() : Ok(impact);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
