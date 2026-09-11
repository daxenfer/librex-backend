using System.Globalization;

namespace Librex.Domain.Entities;

// El folio se muestra siempre con seis dígitos y ceros a la izquierda: 000042. El formato vivía
// repetido en cinco sitios de la capa de aplicación y en un sexto, DeletionRepository, con una
// forma distinta — así que el mismo documento se veía como "000042" o como "42" según quién lo
// pintara. Aquí queda una sola versión, con cultura invariante para que no dependa de la del
// servidor.
public static class Folio
{
    public const int Digits = 6;

    public static string Format(int folioNumber)
        => folioNumber.ToString($"D{Digits}", CultureInfo.InvariantCulture);

    public static string Format(int? folioNumber)
        => folioNumber is { } n ? Format(n) : string.Empty;
}
