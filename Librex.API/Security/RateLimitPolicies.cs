namespace Librex.API.Security;

public static class RateLimitPolicies
{
    // Se aplica solo al login. El resto de la API ya exige un token válido, así que no es una
    // superficie que alguien pueda golpear a ciegas.
    public const string Login = "login";
}
