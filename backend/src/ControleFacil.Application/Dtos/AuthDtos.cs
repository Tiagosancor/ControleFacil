using ControleFacil.Domain.Enums;

namespace ControleFacil.Application.Dtos;

public record RegisterDto(string Name, string Email, string Password);

public record LoginDto(string Email, string Password);

public record UserResponseDto(int Id, string Name, string Email, UserRole Role);

public record AuthResponseDto(string Token, UserResponseDto User);

public record ForgotPasswordDto(string Email);

public record ResetPasswordDto(string Token, string NewPassword);
