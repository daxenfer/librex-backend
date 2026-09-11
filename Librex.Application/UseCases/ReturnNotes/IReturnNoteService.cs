using Librex.Application.DTOs.ReturnNotes;

namespace Librex.Application.UseCases.ReturnNotes;

public interface IReturnNoteService
{
    Task<IEnumerable<ReturnNoteDto>> GetAllAsync(CancellationToken ct = default);
    Task<ReturnNoteDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ReturnNoteDto> CreateAsync(CreateReturnNoteDto dto, CancellationToken ct = default);
    Task<ReturnNoteDto?> UpdateAsync(int id, UpdateReturnNoteDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
