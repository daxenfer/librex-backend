using System.ComponentModel.DataAnnotations;

namespace Librex.Application.Validation;

// Exigencia mínima de complejidad para cualquier contraseña que se fije desde la API.
//
// No pretende ser una política corporativa: con el bloqueo por cuenta activo, lo que frena un
// ataque es que no pueda probar miles de veces, no que la contraseña sea larguísima. Esto solo
// descarta lo indefendible ("12345678", "password").
[AttributeUsage(AttributeTargets.Property)]
public sealed class StrongPasswordAttribute : ValidationAttribute
{
    public const int MinimumLength = 10;

    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        if (value is not string password || string.IsNullOrWhiteSpace(password))
            return new ValidationResult("La contraseña es obligatoria.");

        if (password.Length < MinimumLength)
            return new ValidationResult($"La contraseña debe tener al menos {MinimumLength} caracteres.");

        var missing = new List<string>();
        if (!password.Any(char.IsUpper)) missing.Add("una mayúscula");
        if (!password.Any(char.IsLower)) missing.Add("una minúscula");
        if (!password.Any(char.IsDigit)) missing.Add("un número");
        if (!password.Any(c => !char.IsLetterOrDigit(c))) missing.Add("un símbolo");

        return missing.Count == 0
            ? ValidationResult.Success
            : new ValidationResult($"La contraseña debe incluir {string.Join(", ", missing)}.");
    }
}
