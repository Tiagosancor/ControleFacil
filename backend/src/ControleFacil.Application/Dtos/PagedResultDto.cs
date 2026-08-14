namespace ControleFacil.Application.Dtos;

public record PagedResultDto<T>(int Total, int Page, int PageSize, IReadOnlyList<T> Items);
