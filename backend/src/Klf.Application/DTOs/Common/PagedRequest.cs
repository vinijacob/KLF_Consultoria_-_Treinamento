namespace Klf.Application.DTOs.Common;

public record PagedRequest
{
    public const int MaxPageSize = 100;

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;

    public int Skip => (Page - 1) * PageSize;
}
