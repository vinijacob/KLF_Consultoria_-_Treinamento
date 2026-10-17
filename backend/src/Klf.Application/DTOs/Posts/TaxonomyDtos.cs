namespace Klf.Application.DTOs.Posts;

/// <summary>A category or tag as it appears inside a post: name and address.</summary>
/// <param name="Name">Name shown on the site.</param>
/// <param name="Slug">URL identifier (filter of <c>/conteudo</c>).</param>
public sealed record TaxonomyRef(string Name, string Slug);

/// <summary>A category or tag for the public filters, only when it has visible posts.</summary>
/// <param name="Name">Name shown on the site.</param>
/// <param name="Slug">URL identifier.</param>
/// <param name="PostCount">How many visible posts use it.</param>
public sealed record PublicTaxonomyItemResponse(string Name, string Slug, int PostCount);

/// <summary>A category or tag in the admin panel.</summary>
/// <param name="Id">Identifier.</param>
/// <param name="Name">Name shown on the site.</param>
/// <param name="Slug">URL identifier, made from the name by the server.</param>
/// <param name="PostCount">How many posts use it, drafts included.</param>
public sealed record TaxonomyItemResponse(Guid Id, string Name, string Slug, int PostCount);

/// <summary>Fields shared by the requests that name a category or tag.</summary>
public interface ITaxonomyNameFields
{
    /// <summary>Name shown on the site; the address (slug) is made from it.</summary>
    string Name { get; }
}

/// <summary>Creates a category.</summary>
/// <param name="Name">Name shown on the site (up to 60 characters).</param>
public sealed record CreateCategoryRequest(string Name) : ITaxonomyNameFields;

/// <summary>Renames a category.</summary>
/// <param name="Name">Name shown on the site (up to 60 characters).</param>
public sealed record UpdateCategoryRequest(string Name) : ITaxonomyNameFields;

/// <summary>Renames a tag.</summary>
/// <param name="Name">Name shown on the site (up to 40 characters).</param>
public sealed record UpdateTagRequest(string Name) : ITaxonomyNameFields;
