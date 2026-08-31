using System.ComponentModel.DataAnnotations;

namespace Librex.Application.DTOs.Remissions;

// Sin campos propios: IsActive no lo edita el usuario, solo lo mueve el borrado lógico.
public class UpdateRemissionDto : CreateRemissionDto
{
}
