namespace Librex.Domain.Exceptions;

// Una regla de negocio que el usuario violó — no una falla del sistema. El middleware la traduce
// a un 400 con este mensaje y, a propósito, NO la registra en error_logs.
//
// No se llama ValidationException para no chocar con la de System.ComponentModel.DataAnnotations,
// que los DTOs de esta solución ya usan.
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}
