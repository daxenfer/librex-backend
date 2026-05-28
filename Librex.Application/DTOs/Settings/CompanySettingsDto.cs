namespace Librex.Application.DTOs.Settings;

public class CompanySettingsDto
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string Rfc { get; set; } = string.Empty;
    public string? Phone1 { get; set; }
    public string? Phone2 { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? PostalCode { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
}
