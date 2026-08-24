using ControleFacil.Domain.Enums;

namespace ControleFacil.Application.Dtos;

public record UsageEventCreateDto(UsageEventType EventType, string? Metadata);

public record UsageEventResponseDto(int Id, int UserId, UsageEventType EventType, string? Metadata, DateTime CreatedAt);

public record LoginHistoryItemDto(int UserId, string UserName, string UserEmail, DateTime CreatedAt);

public record LoggedInUserDto(int UserId, string UserName, string UserEmail, DateTime LastLoginAt);
