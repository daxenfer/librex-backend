using Librex.Application.DTOs.ReturnNotes;

namespace Librex.Application.UseCases.ReturnNotes;

public interface IReturnNoteService
{
    Task<IEnumerable<ReturnNoteDto>> GetAllAsync();
    Task<ReturnNoteDto?> GetByIdAsync(int id);
    Task<ReturnNoteDto> CreateAsync(CreateReturnNoteDto dto);
    Task<ReturnNoteDto?> UpdateAsync(int id, UpdateReturnNoteDto dto);
    Task<bool> DeleteAsync(int id);
}
