namespace Librex.Application.UseCases.Users;

// Quién está ejecutando la operación, tal como viene en los claims del token. UserService lo
// necesita para las reglas que dependen de la sesión: no degradarse a sí mismo, no tocar a
// alguien de mayor rango, no dejar el sistema sin administrador.
public record ActingUser(int Id, string Role);
