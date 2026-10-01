namespace Klf.Application.DTOs.Common;

/// <summary>
/// Pagination parameters for list endpoints. Inherit from it when a list also needs filters.
/// </summary>
public record PagedRequest
{
    /// <summary>Largest page size a client may request.</summary>
    public const int MaxPageSize = 100;

    /// <summary>Page number, starting at 1.</summary>
    public int Page { get; init; } = 1;

    /// <summary>Number of items per page, from 1 to <see cref="MaxPageSize"/>.</summary>
    public int PageSize { get; init; } = 20;

    /// <summary>Number of items to skip in the query, derived from <see cref="Page"/> and <see cref="PageSize"/>.</summary>
    public int Skip => (Page - 1) * PageSize;
}
