using System.Security.Claims;
using ControleFacil.Domain.Enums;

namespace ControleFacil.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        var idClaim = principal.FindFirst("id")
            ?? throw new InvalidOperationException("Claim 'id' não encontrada no token.");
        return int.Parse(idClaim.Value);
    }

    // Sem fallback silencioso: um token sem a claim "role" (ex.: emitido antes desta
    // mudança) ou com um valor que não bate com nenhum UserRole lança exceção em vez de
    // assumir User ou Admin por padrão — evita elevação/perda de privilégio silenciosa.
    public static UserRole GetRole(this ClaimsPrincipal principal)
    {
        var roleClaim = principal.FindFirst("role")
            ?? throw new InvalidOperationException("Claim 'role' não encontrada no token.");
        return Enum.Parse<UserRole>(roleClaim.Value);
    }
}
