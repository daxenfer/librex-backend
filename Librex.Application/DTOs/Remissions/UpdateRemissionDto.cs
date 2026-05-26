using System.ComponentModel.DataAnnotations;

namespace Librex.Application.DTOs.Remissions;

public class UpdateRemissionDto : CreateRemissionDto
{
    public bool IsActive { get; set; } = true;
}
