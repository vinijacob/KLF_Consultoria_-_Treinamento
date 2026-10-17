using Klf.Domain.Common;

namespace Klf.Domain.Entities;

/// <summary>
/// The theme of a post (e.g. Atendimento, Liderança). A post has at most one; deleting a category leaves its posts without
/// one. Different from <see cref="Enums.PostType"/>, which says the format (article, project, news).
/// </summary>
public sealed class Category : Entity
{
    /// <summary>Creates a category.</summary>
    /// <param name="name">Name shown on the site.</param>
    /// <param name="slug">Unique URL identifier (used in <c>/conteudo?categoria=...</c>).</param>
    public Category(string name, string slug)
    {
        Name = name;
        Slug = slug;
    }

    /// <summary>Name shown on the site.</summary>
    public string Name { get; private set; }

    /// <summary>Unique, URL-friendly identifier.</summary>
    public string Slug { get; private set; }

    /// <summary>Renames the category.</summary>
    public void Rename(string name, string slug)
    {
        Name = name;
        Slug = slug;
    }
}
