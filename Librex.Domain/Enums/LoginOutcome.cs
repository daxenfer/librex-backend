namespace Librex.Domain.Enums;

// Por qué terminó como terminó un intento de acceso. Al usuario siempre se le responde lo mismo
// (credenciales inválidas); el motivo real solo queda en la bitácora, para poder distinguir un
// dedo torpe de un barrido de contraseñas.
public enum LoginOutcome
{
    Success,
    UnknownUser,
    InactiveUser,
    BadPassword,
    LockedOut,
}
