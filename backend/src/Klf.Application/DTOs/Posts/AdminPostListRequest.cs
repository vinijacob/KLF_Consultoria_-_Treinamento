using Klf.Domain.Enums;

namespace Klf.Application.DTOs.Posts;

/// <summary>Pagination and filters for the admin post list; also filters by status.</summary>
public sealed record AdminPostListRequest : PostListRequest
{
    /// <summary>Only posts with this status.</summary>
    public PostStatus? Status { get; init; }
}
