namespace Librex.Application.DTOs.Deletion;

// Lo que provoca eliminar una entidad, para confirmarlo con el usuario.
// Items: lo que se elimina junto con ella. PreservedItems: los documentos ya emitidos que la
// siguen citando y que no se modifican.
public class DeletionImpactDto
{
    public string EntityType { get; set; } = string.Empty;
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public List<DeletionImpactItemDto> Items { get; set; } = [];
    public int TotalDependents { get; set; }
    public List<DeletionImpactItemDto> PreservedItems { get; set; } = [];
    public int TotalPreserved { get; set; }
}
