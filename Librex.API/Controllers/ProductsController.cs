using Librex.Application.DTOs.Products;
using Librex.Application.UseCases.Products;
using Librex.Application.DTOs.Deletion;
using Librex.Application.UseCases.Deletion;
using Librex.Domain.Constants;
using Librex.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Librex.API.Controllers;

[Authorize]
[ApiController]
[Route("api/products")]
public sealed class ProductsController(IProductService service, IDeletionService deletionService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll()
        => Ok(await service.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var product = await service.GetByIdAsync(id);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.ProductsWrite)]
    public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductDto dto)
    {
        var created = await service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = Permissions.ProductsWrite)]
    public async Task<ActionResult<ProductDto>> Update(int id, [FromBody] UpdateProductDto dto)
    {
        var updated = await service.UpdateAsync(id, dto);
        return updated is null ? NotFound() : Ok(updated);
    }

    // Qué se va a borrar en cascada junto con esta entidad. Se consulta antes del DELETE
    // para que el usuario confirme con el impacto a la vista.
    [HttpGet("{id:int}/deletion-impact")]
    public async Task<ActionResult<DeletionImpactDto>> GetDeletionImpact(int id)
    {
        var impact = await deletionService.GetImpactAsync(DeletableEntity.Product, id);
        return impact is null ? NotFound() : Ok(impact);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Permissions.ProductsDelete)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
