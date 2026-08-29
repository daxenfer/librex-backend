namespace Librex.Application.DTOs.Deletion;

// Un renglón del impacto: cuántos registros de un tipo caen. EntityName ya viene en español
// para mostrarse tal cual en la UI.
public class DeletionImpactItemDto
{
    public string EntityName { get; set; } = string.Empty;
    public int Count { get; set; }
}
