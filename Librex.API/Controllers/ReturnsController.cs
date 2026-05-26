using Librex.Application.DTOs.ReturnNotes;
using Librex.Application.UseCases.ReturnNotes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Librex.API.Controllers;

[Authorize]
[ApiController]
[Route("api/returns")]
public class ReturnsController : ControllerBase
{
    private readonly IReturnNoteService _service;

    public ReturnsController(IReturnNoteService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReturnNoteDto>>> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReturnNoteDto>> GetById(int id)
    {
        var note = await _service.GetByIdAsync(id);
        return note is null ? NotFound() : Ok(note);
    }

    [HttpPost]
    public async Task<ActionResult<ReturnNoteDto>> Create([FromBody] CreateReturnNoteDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ReturnNoteDto>> Update(int id, [FromBody] UpdateReturnNoteDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
