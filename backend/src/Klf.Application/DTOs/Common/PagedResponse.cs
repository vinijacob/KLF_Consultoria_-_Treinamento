namespace Klf.Application.DTOs.Common;

/// <summary>A page of results plus the metadata the frontend needs to render pagination.</summary>
/// <typeparam name="T">Item DTO type.</typeparam>
/// <param name="Items">Items in the current page.</param>
/// <param name="Page">Current page number, starting at 1.</param>
/// <param name="PageSize">Requested number of items per page.</param>
/// <param name="TotalItems">Total number of items across all pages.</param>
public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalItems)
{
    /// <summary>Total number of pages.</summary>
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize);

    /// <summary>Whether there is a page after the current one.</summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>Whether there is a page before the current one.</summary>
    public bool HasPreviousPage => Page > 1;
}
