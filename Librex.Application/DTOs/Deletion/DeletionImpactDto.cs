namespace Librex.Application.DTOs.Deletion;

// Lo que se va a borrar en cascada junto con una entidad, para confirmarlo con el usuario.
public class DeletionImpactDto
{
    public string EntityType { get; set; } = string.Empty;
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public List<DeletionImpactItemDto> Items { get; set; } = [];
    public int TotalDependents { get; set; }
}
