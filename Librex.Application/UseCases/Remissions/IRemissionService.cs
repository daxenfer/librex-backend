using Librex.Application.DTOs.Remissions;

namespace Librex.Application.UseCases.Remissions;

public interface IRemissionService
{
    Task<IEnumerable<RemissionDto>> GetAllAsync(CancellationToken ct = default);
    Task<RemissionDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<RemissionDto> CreateAsync(CreateRemissionDto dto, CancellationToken ct = default);
    Task<RemissionDto?> UpdateAsync(int id, UpdateRemissionDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
