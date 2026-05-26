namespace Librex.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int PublisherId { get; set; }
    public Publisher Publisher { get; set; } = null!;
}
