namespace SupportDesk.Core.Dtos;

public record PagedResultDto<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize
);