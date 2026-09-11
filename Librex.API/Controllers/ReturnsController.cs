using Librex.Application.DTOs.ReturnNotes;
using Librex.Application.UseCases.ReturnNotes;
using Librex.Application.DTOs.Deletion;
using Librex.Application.UseCases.Deletion;
using Librex.Domain.Constants;
using Librex.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Librex.API.Controllers;

[Authorize]
[ApiController]
[Route("api/returns")]
public sealed class ReturnsController(IReturnNoteService service, IDeletionService deletionService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReturnNoteDto>>> GetAll(CancellationToken ct)
        => Ok(await service.GetAllAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReturnNoteDto>> GetById(int id, CancellationToken ct)
    {
        var note = await service.GetByIdAsync(id, ct);
        return note is null ? NotFound() : Ok(note);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.ReturnsWrite)]
    public async Task<ActionResult<ReturnNoteDto>> Create([FromBody] CreateReturnNoteDto dto, CancellationToken ct)
    {
        var created = await service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = Permissions.ReturnsWrite)]
    public async Task<ActionResult<ReturnNoteDto>> Update(int id, [FromBody] UpdateReturnNoteDto dto, CancellationToken ct)
    {
        var updated = await service.UpdateAsync(id, dto, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    // Qué se va a borrar en cascada junto con esta entidad. Se consulta antes del DELETE
    // para que el usuario confirme con el impacto a la vista.
    [HttpGet("{id:int}/deletion-impact")]
    public async Task<ActionResult<DeletionImpactDto>> GetDeletionImpact(int id, CancellationToken ct)
    {
        var impact = await deletionService.GetImpactAsync(DeletableEntity.ReturnNote, id, ct);
        return impact is null ? NotFound() : Ok(impact);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Permissions.ReturnsDelete)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await service.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }
}
