using System.Security.Cryptography;
using ControleFacil.Application.Dtos;
using ControleFacil.Application.Exceptions;
using ControleFacil.Application.Interfaces;
using ControleFacil.Domain.Entities;
using ControleFacil.Domain.Enums;
using ControleFacil.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ControleFacil.Application.Services;

public class AuthService : IAuthService
{
    private static readonly TimeSpan ResetTokenLifetime = TimeSpan.FromMinutes(45);

    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
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

        return new UserResponseDto(user.Id, user.Name, user.Email, user.Role);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(dto.Email);
        if (user is null || !_passwordHasher.VerifyPassword(user, dto.Password))
            throw new AuthenticationException("Email ou senha inválidos");

        var token = _jwtTokenService.GenerateToken(user);

        // Único ponto do sistema que grava um Login de verdade — sem isso, login-history
        // e logged-in-users (Sprint Admin-1) nunca refletem uso real, só os eventos de
        // teste inseridos manualmente via POST /api/usage-events.
        await _unitOfWork.UsageEvents.AddAsync(new UsageEvent
        {
            UserId = user.Id,
            EventType = UsageEventType.Login,
            CreatedAt = DateTime.UtcNow,
        });
        await _unitOfWork.SaveChangesAsync();

        return new AuthResponseDto(token, new UserResponseDto(user.Id, user.Name, user.Email, user.Role));
    }

    public async Task ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(dto.Email);
        if (user is null)
            return; // resposta genérica é responsabilidade do endpoint — aqui só não há o que fazer

        var activeTokens = await _unitOfWork.PasswordResetTokens.Query()
            .Where(t => t.UserId == user.Id && t.UsedAt == null)
            .ToListAsync();
        foreach (var activeToken in activeTokens)
        {
            activeToken.UsedAt = DateTime.UtcNow;
            _unitOfWork.PasswordResetTokens.Update(activeToken);
        }

        var rawToken = GenerateRawToken();
        var resetToken = new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = HashToken(rawToken),
            ExpiresAt = DateTime.UtcNow.Add(ResetTokenLifetime),
            CreatedAt = DateTime.UtcNow,
        };
        await _unitOfWork.PasswordResetTokens.AddAsync(resetToken);
        await _unitOfWork.SaveChangesAsync();

        var frontendBaseUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:3000";
        var resetLink = $"{frontendBaseUrl.TrimEnd('/')}/reset-password?token={rawToken}";

        try
        {
            await _emailService.SendPasswordResetEmailAsync(user.Email, resetLink);
        }
        catch (Exception ex)
        {
            // O endpoint sempre responde com sucesso genérico (não pode revelar se o envio falhou),
            // então uma falha no Resend (ex.: domínio ainda não verificado) só fica registrada no log.
            _logger.LogError(ex, "Falha ao enviar e-mail de recuperação de senha para o usuário {UserId}", user.Id);
        }
    }

    public async Task ResetPasswordAsync(ResetPasswordDto dto)
    {
        var tokenHash = HashToken(dto.Token);
        var resetToken = await _unitOfWork.PasswordResetTokens.GetByTokenHashAsync(tokenHash);

        if (resetToken is null || resetToken.UsedAt is not null || resetToken.ExpiresAt < DateTime.UtcNow)
            throw new BusinessRuleException("Token inválido ou expirado");

        var user = await _unitOfWork.Users.GetByIdAsync(resetToken.UserId)
            ?? throw new BusinessRuleException("Token inválido ou expirado");

        user.PasswordHash = _passwordHasher.HashPassword(user, dto.NewPassword);
        _unitOfWork.Users.Update(user);

        resetToken.UsedAt = DateTime.UtcNow;
        _unitOfWork.PasswordResetTokens.Update(resetToken);

        await _unitOfWork.SaveChangesAsync();
    }

    private static string GenerateRawToken() =>
        Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

    private static string HashToken(string rawToken) =>
        Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawToken)));
}
