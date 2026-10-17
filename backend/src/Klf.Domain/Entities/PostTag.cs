using Klf.Domain.Common;

namespace Klf.Domain.Entities;

/// <summary>Links a <see cref="Post"/> to one of its <see cref="Entities.Tag"/>s.</summary>
public sealed class PostTag : Entity
{
    /// <summary>Links the post being edited to a tag.</summary>
    /// <param name="tag">The tag (new or already saved).</param>
    public PostTag(Tag tag)
    {
        Tag = tag;
        TagId = tag.Id;
    }

    private PostTag()
    {
        Tag = null!;
    }

    /// <summary>The post.</summary>
    public Guid PostId { get; private set; }

    /// <summary>The tag.</summary>
    public Guid TagId { get; private set; }

    /// <summary>The tag, loaded with the post.</summary>
    public Tag Tag { get; private set; }
}
