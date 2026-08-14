namespace ControleFacil.Application.Exceptions;

/// <summary>
/// Violação de uma regra de negócio (ex.: hierarquia de categoria com mais de 2 níveis).
/// Mapeada para 400 Bad Request pelo middleware global.
/// </summary>
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message)
    {
    }
}
