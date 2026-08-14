using ControleFacil.Application.Dtos;
using ControleFacil.Application.Exceptions;
using ControleFacil.Application.Interfaces;
using ControleFacil.Domain.Entities;
using ControleFacil.Domain.Interfaces;

namespace ControleFacil.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, IJwtTokenService jwtTokenService)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<UserResponseDto> RegisterAsync(RegisterDto dto)
    {
        if (await _unitOfWork.Users.AnyAsync(u => u.Email == dto.Email))
            throw new ConflictException("Email já cadastrado");

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            CreatedAt = DateTime.UtcNow,
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return new UserResponseDto(user.Id, user.Name, user.Email);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(dto.Email);
        if (user is null || !_passwordHasher.VerifyPassword(user, dto.Password))
            throw new AuthenticationException("Email ou senha inválidos");

        var token = _jwtTokenService.GenerateToken(user);
        return new AuthResponseDto(token, new UserResponseDto(user.Id, user.Name, user.Email));
    }
}
