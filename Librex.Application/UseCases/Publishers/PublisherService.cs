using Librex.Application.DTOs.Publishers;
using Librex.Domain.Entities;
using Librex.Domain.Interfaces;

namespace Librex.Application.UseCases.Publishers;

public class PublisherService : IPublisherService
{
    private readonly IPublisherRepository _repository;

    public PublisherService(IPublisherRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<PublisherDto>> GetAllAsync()
        => (await _repository.GetAllAsync()).Select(MapToDto);

    public async Task<PublisherDto?> GetByIdAsync(int id)
    {
        var publisher = await _repository.GetByIdAsync(id);
        return publisher is null ? null : MapToDto(publisher);
    }

    public async Task<PublisherDto> CreateAsync(CreatePublisherDto dto)
    {
        var publisher = new Publisher
        {
            Name = dto.Name,
            Contact = dto.Contact,
            Phone = dto.Phone,
            Email = dto.Email,
        };
        return MapToDto(await _repository.AddAsync(publisher));
    }

    public async Task<PublisherDto?> UpdateAsync(int id, UpdatePublisherDto dto)
    {
        var publisher = await _repository.GetByIdAsync(id);
        if (publisher is null) return null;

        publisher.Name = dto.Name;
        publisher.Contact = dto.Contact;
        publisher.Phone = dto.Phone;
        publisher.Email = dto.Email;
        publisher.IsActive = dto.IsActive;

        await _repository.UpdateAsync(publisher);
        return MapToDto(publisher);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var publisher = await _repository.GetByIdAsync(id);
        if (publisher is null) return false;
        await _repository.DeleteAsync(id);
        return true;
    }

    private static PublisherDto MapToDto(Publisher p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Contact = p.Contact,
        Phone = p.Phone,
        Email = p.Email,
        IsActive = p.IsActive,
    };
}
