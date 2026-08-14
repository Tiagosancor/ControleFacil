using ControleFacil.Domain.Entities;

namespace ControleFacil.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}
