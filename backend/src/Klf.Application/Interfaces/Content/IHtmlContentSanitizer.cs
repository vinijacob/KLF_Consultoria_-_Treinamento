namespace Klf.Application.Interfaces.Content;

/// <summary>Removes unsafe markup from HTML written in the rich text editor. Implemented in Infrastructure.</summary>
public interface IHtmlContentSanitizer
{
    /// <summary>
    /// Returns the HTML without scripts, event handlers (<c>onclick</c>...) and other XSS vectors,
    /// keeping formatting tags such as headings, lists, links and images.
    /// </summary>
    string Sanitize(string html);
}
