using Ganss.Xss;

using Klf.Application.Interfaces.Content;

namespace Klf.Infrastructure.Services;

/// <summary>
/// Cleans the HTML of the rich text editor before it is saved. Only what the editor produces survives: the usual text tags,
/// links, images, <c>text-align</c> as the single CSS property and the site's own classes (list markers and text colors),
/// so pasted or crafted HTML cannot restyle or break the public pages.
/// </summary>
internal sealed class HtmlContentSanitizer : IHtmlContentSanitizer
{
    /// <summary>Classes the editor may emit; every other class is removed.</summary>
    public static IReadOnlyList<string> AllowedClasses { get; } =
    [
        "klf-list-disc", "klf-list-circle", "klf-list-dash",
        "klf-list-lower-alpha", "klf-list-upper-alpha", "klf-list-lower-roman", "klf-list-upper-roman",
        "klf-color-brand", "klf-color-accent",
    ];

    public string Sanitize(string html) => Create().Sanitize(html);

    private static HtmlSanitizer Create()
    {
        var sanitizer = new HtmlSanitizer();

        sanitizer.AllowedCssProperties.Clear();
        sanitizer.AllowedCssProperties.Add("text-align");
        sanitizer.AllowedAttributes.Add("class");
        sanitizer.AllowedAttributes.Add("type");
        sanitizer.AllowedAttributes.Add("start");

        foreach (var name in AllowedClasses)
        {
            sanitizer.AllowedClasses.Add(name);
        }

        sanitizer.AllowedSchemes.Clear();
        sanitizer.AllowedSchemes.Add("https");
        sanitizer.AllowedSchemes.Add("mailto");

        return sanitizer;
    }
}
