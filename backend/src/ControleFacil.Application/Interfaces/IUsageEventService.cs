using ControleFacil.Application.Dtos;

namespace ControleFacil.Application.Interfaces;

public interface IUsageEventService
{
    Task<PagedResultDto<LoginHistoryItemDto>> GetLoginHistoryAsync(int? userId, int page, int pageSize);
    Task<IReadOnlyList<LoggedInUserDto>> GetLoggedInUsersAsync(int? minutes);
}
