using Librex.Application.DTOs.Remissions;

namespace Librex.Application.UseCases.Remissions;

public interface IRemissionService
{
    Task<IEnumerable<RemissionDto>> GetAllAsync();
    Task<RemissionDto?> GetByIdAsync(int id);
    Task<RemissionDto> CreateAsync(CreateRemissionDto dto);
    Task<RemissionDto?> UpdateAsync(int id, UpdateRemissionDto dto);
    Task<bool> DeleteAsync(int id);
}
