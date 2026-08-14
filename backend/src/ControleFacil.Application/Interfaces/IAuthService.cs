using ControleFacil.Application.Dtos;

namespace ControleFacil.Application.Interfaces;

public interface IAuthService
{
    Task<UserResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
}
