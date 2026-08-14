using System.Security.Claims;

namespace ControleFacil.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        var idClaim = principal.FindFirst("id")
            ?? throw new InvalidOperationException("Claim 'id' não encontrada no token.");
        return int.Parse(idClaim.Value);
    }
}
