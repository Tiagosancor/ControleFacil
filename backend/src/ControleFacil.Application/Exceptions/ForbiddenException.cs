namespace ControleFacil.Application.Exceptions;

/// <summary>
/// Ação bloqueada por regra de negócio explícita (ex.: editar/excluir uma categoria de
/// sistema), não por ownership/existência — por isso mapeada para 403, diferente de
/// NotFoundException (que evita vazar a existência de recursos de outro usuário).
/// </summary>
public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message)
    {
    }
}
