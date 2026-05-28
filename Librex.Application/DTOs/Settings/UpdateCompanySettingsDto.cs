using System.ComponentModel.DataAnnotations;

namespace Librex.Application.DTOs.Settings;

public class UpdateCompanySettingsDto
{
    [Required]
    [MaxLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string BrandName { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Rfc { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Phone1 { get; set; }

    [MaxLength(50)]
    public string? Phone2 { get; set; }

    [MaxLength(200)]
    public string? Email { get; set; }

    [MaxLength(300)]
    public string? Address { get; set; }

    [MaxLength(10)]
    public string? PostalCode { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? State { get; set; }
}
