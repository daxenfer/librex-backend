using Librex.Application.DTOs.Deletion;
using Librex.Domain.Enums;

namespace Librex.Application.UseCases.Deletion;

public interface IDeletionService
{
    Task<DeletionImpactDto?> GetImpactAsync(DeletableEntity entity, int id);
}
