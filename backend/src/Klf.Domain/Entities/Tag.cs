using Klf.Domain.Common;

namespace Klf.Domain.Entities;

/// <summary>A free keyword of posts (e.g. pós-venda). Created when first typed in a post; a post can have several.</summary>
public sealed class Tag : Entity
{
    /// <summary>Creates a tag.</summary>
    /// <param name="name">Name shown on the site.</param>
    /// <param name="slug">Unique URL identifier (used in <c>/conteudo?tag=...</c>).</param>
    public Tag(string name, string slug)
    {
        Name = name;
        Slug = slug;
    }

    /// <summary>Name shown on the site.</summary>
    public string Name { get; private set; }

    /// <summary>Unique, URL-friendly identifier.</summary>
    public string Slug { get; private set; }

    /// <summary>Renames the tag.</summary>
    public void Rename(string name, string slug)
    {
        Name = name;
        Slug = slug;
    }
}
