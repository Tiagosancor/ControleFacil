using ControleFacil.Application.Interfaces;
using ControleFacil.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace ControleFacil.Infrastructure.Auth;

public class PasswordHasherService : IPasswordHasher
{
    private readonly PasswordHasher<User> _hasher = new();

    public string HashPassword(User user, string password) => _hasher.HashPassword(user, password);

    public bool VerifyPassword(User user, string password) =>
        _hasher.VerifyHashedPassword(user, user.PasswordHash, password) != PasswordVerificationResult.Failed;
}
