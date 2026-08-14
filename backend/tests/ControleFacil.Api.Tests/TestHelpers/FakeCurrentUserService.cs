using ControleFacil.Application.Interfaces;

namespace ControleFacil.Api.Tests.TestHelpers;

public class FakeCurrentUserService : ICurrentUserService
{
    public FakeCurrentUserService(int userId) => UserId = userId;

    public int UserId { get; set; }
}
