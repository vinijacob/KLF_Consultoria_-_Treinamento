using Klf.Application.DTOs.Common;
using Klf.Domain.Enums;

namespace Klf.Application.DTOs.Posts;

/// <summary>Pagination and filters for the public post list (query string: <c>?type=News&amp;search=lideranca&amp;page=2</c>).</summary>
public record PostListRequest : PagedRequest
{
    /// <summary>Only posts of this kind.</summary>
    public PostType? Type { get; init; }

    /// <summary>Keyword searched in the title and summary.</summary>
    public string? Search { get; init; }

    /// <summary>Only posts of the category with this slug.</summary>
    public string? Category { get; init; }

    /// <summary>Only posts with the tag of this slug.</summary>
    public string? Tag { get; init; }
}
