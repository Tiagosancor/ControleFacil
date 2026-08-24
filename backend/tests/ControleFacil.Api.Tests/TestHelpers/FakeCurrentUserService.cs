using ControleFacil.Application.Interfaces;
using ControleFacil.Domain.Enums;

namespace ControleFacil.Api.Tests.TestHelpers;

public class FakeCurrentUserService : ICurrentUserService
{
    public FakeCurrentUserService(int userId, UserRole role = UserRole.User)
    {
        UserId = userId;
        Role = role;
    }

    public int UserId { get; set; }
    public UserRole Role { get; set; }
}
