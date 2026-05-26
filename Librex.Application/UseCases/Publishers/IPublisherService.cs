using Librex.Application.DTOs.Publishers;

namespace Librex.Application.UseCases.Publishers;

public interface IPublisherService
{
    Task<IEnumerable<PublisherDto>> GetAllAsync();
    Task<PublisherDto?> GetByIdAsync(int id);
    Task<PublisherDto> CreateAsync(CreatePublisherDto dto);
    Task<PublisherDto?> UpdateAsync(int id, UpdatePublisherDto dto);
    Task<bool> DeleteAsync(int id);
}
