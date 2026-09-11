using System.Security.Claims;
using Librex.Application.DTOs.Users;
using Librex.Application.UseCases.Users;
using Librex.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Librex.API.Controllers;

// Solo SuperAdmin: users.manage no lo tiene ningún rol del cliente. El permiso va a nivel de
// clase porque aquí ni siquiera la lectura es pública para el resto de los roles.
[Authorize(Policy = Permissions.UsersManage)]
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _service;

    public UsersController(IUserService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDto>> GetById(int id)
    {
        var user = await _service.GetByIdAsync(id);
        return user is null ? NotFound() : Ok(user);
    }

    // Alimenta las dos cosas que la pantalla de usuarios necesita saber de la autorización: qué
    // roles se pueden asignar y qué puede hacer cada uno. Es de solo lectura — la matriz está
    // compilada.
    [HttpGet("permission-matrix")]
    public ActionResult<PermissionMatrixDto> GetPermissionMatrix() => Ok(_service.GetPermissionMatrix());

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserDto dto)
    {
        var created = await _service.CreateAsync(dto, CurrentUser());
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserDto>> Update(int id, [FromBody] UpdateUserDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto, CurrentUser());
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpPut("{id:int}/password")]
    public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordDto dto)
    {
        var changed = await _service.ChangePasswordAsync(id, dto, CurrentUser());
        return changed ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id, CurrentUser());
        return deleted ? NoContent() : NotFound();
    }

    // El handler de JWT mapea `sub` a NameIdentifier por omisión, pero se leen los dos por si esa
    // configuración cambia. Sin id no se puede aplicar ninguna regla de auto-protección, así que
    // se falla en vez de asumir.
    private ActingUser CurrentUser()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!int.TryParse(raw, out var id))
            throw new InvalidOperationException("El token no trae el identificador del usuario.");

        return new ActingUser(id, User.FindFirstValue(ClaimTypes.Role) ?? string.Empty);
    }
}
