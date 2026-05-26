using Librex.Application.DTOs.Remissions;
using Librex.Application.UseCases.Remissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Librex.API.Controllers;

[Authorize]
[ApiController]
[Route("api/remissions")]
public class RemissionsController : ControllerBase
{
    private readonly IRemissionService _service;

    public RemissionsController(IRemissionService service)
    {
        _service = service;
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

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
