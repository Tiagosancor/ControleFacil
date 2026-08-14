using ControleFacil.Api.Extensions;
using ControleFacil.Application.Interfaces;

namespace ControleFacil.Api.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User
                ?? throw new InvalidOperationException("Nenhum HttpContext disponível para resolver o usuário atual.");
            return user.GetUserId();
        }
    }
}
