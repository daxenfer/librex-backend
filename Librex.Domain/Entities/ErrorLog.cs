namespace Librex.Domain.Entities;

// Registro inmutable de una excepción no manejada. No hereda de BaseEntity: no aplica
// la semántica de IsActive/ModifiedAt de las entidades editables, es solo un log de una escritura.
public class ErrorLog
{
    public int Id { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string RequestId { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string? QueryString { get; set; }
    public string? RouteValues { get; set; }
    public string? RequestBody { get; set; }
    public int StatusCode { get; set; }
    public string ExceptionType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public string? Username { get; set; }
}
