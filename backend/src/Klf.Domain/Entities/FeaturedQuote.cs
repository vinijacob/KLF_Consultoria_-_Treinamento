using Klf.Domain.Common;

namespace Klf.Domain.Entities;

/// <summary>
/// The quote of Kilciene highlighted on the home page. There is only one: saving again replaces it.
/// </summary>
public sealed class FeaturedQuote : Entity
{
    /// <summary>Creates the quote.</summary>
    /// <param name="text">The quote itself, plain text.</param>
    /// <param name="author">Who said it.</param>
    /// <param name="source">Where or when it was said (e.g. a talk); optional.</param>
    /// <param name="isPublished">Whether the home page shows it.</param>
    public FeaturedQuote(string text, string author, string? source, bool isPublished)
    {
        Text = text;
        Author = author;
        Source = source;
        IsPublished = isPublished;
    }

    /// <summary>The quote itself, plain text.</summary>
    public string Text { get; private set; }

    /// <summary>Who said it.</summary>
    public string Author { get; private set; }

    /// <summary>Where or when it was said; optional.</summary>
    public string? Source { get; private set; }

    /// <summary>Whether the home page shows it.</summary>
    public bool IsPublished { get; private set; }

    /// <summary>Replaces every field.</summary>
    public void Update(string text, string author, string? source, bool isPublished)
    {
        Text = text;
        Author = author;
        Source = source;
        IsPublished = isPublished;
    }
}
