using ControleFacil.Domain.Entities;

namespace ControleFacil.Application.Interfaces;

public interface IPasswordHasher
{
    string HashPassword(User user, string password);
    bool VerifyPassword(User user, string password);
}
