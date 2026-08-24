using ControleFacil.Domain.Enums;

namespace ControleFacil.Application.Interfaces;

public interface ICurrentUserService
{
    int UserId { get; }
    UserRole Role { get; }
}
