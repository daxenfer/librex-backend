namespace Librex.Application.DTOs.ReturnNotes;

public class UpdateReturnNoteDto : CreateReturnNoteDto
{
    public bool IsActive { get; set; } = true;
}
