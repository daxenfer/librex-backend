namespace Librex.Domain.Entities;

public class Publisher : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Contact { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
