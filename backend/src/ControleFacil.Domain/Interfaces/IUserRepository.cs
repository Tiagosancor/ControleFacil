using ControleFacil.Domain.Entities;

namespace ControleFacil.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
}
