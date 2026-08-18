using ControleFacil.Domain.Entities;

namespace ControleFacil.Domain.Interfaces;

public interface IPasswordResetTokenRepository : IRepository<PasswordResetToken>
{
    Task<PasswordResetToken?> GetByTokenHashAsync(string tokenHash);
}
