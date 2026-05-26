namespace Librex.Application.DTOs.Publishers;

public class UpdatePublisherDto : CreatePublisherDto
{
    public bool IsActive { get; set; } = true;
}
