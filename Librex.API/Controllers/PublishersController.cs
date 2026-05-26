using Librex.Application.DTOs.Publishers;
using Librex.Application.UseCases.Publishers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Librex.API.Controllers;

[Authorize]
[ApiController]
[Route("api/publishers")]
public class PublishersController : ControllerBase
{
    private readonly IPublisherService _service;

    public PublishersController(IPublisherService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PublisherDto>>> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PublisherDto>> GetById(int id)
    {
        var publisher = await _service.GetByIdAsync(id);
        return publisher is null ? NotFound() : Ok(publisher);
    }

    [HttpPost]
    public async Task<ActionResult<PublisherDto>> Create([FromBody] CreatePublisherDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PublisherDto>> Update(int id, [FromBody] UpdatePublisherDto dto)
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
