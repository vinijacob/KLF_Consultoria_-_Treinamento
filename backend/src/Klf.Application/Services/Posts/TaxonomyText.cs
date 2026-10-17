using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Klf.Application.Services.Posts;

/// <summary>Cleans category and tag names and makes their address: "Pós-venda " → "Pós-venda" and "pos-venda".</summary>
public static partial class TaxonomyText
{
    /// <summary>Largest category name.</summary>
    public const int CategoryNameMaxLength = 60;

    /// <summary>Largest tag name.</summary>
    public const int TagNameMaxLength = 40;

    /// <summary>Largest slug, matching the database column.</summary>
    public const int SlugMaxLength = 80;

    /// <summary>Trims the name and collapses inner spaces.</summary>
    public static string CleanName(string name) => Spaces().Replace(name.Trim(), " ");

    /// <summary>Lowercase, no accents, words joined by hyphens; empty when the name has no letter or digit.</summary>
    public static string ToSlug(string name)
    {
        var builder = new StringBuilder(name.Length);

        foreach (var character in name.Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(char.ToLowerInvariant(character));
            }
        }

        var slug = NotAlphanumeric().Replace(builder.ToString(), "-").Trim('-');

        return slug.Length > SlugMaxLength ? slug[..SlugMaxLength].TrimEnd('-') : slug;
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex Spaces();

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NotAlphanumeric();
}
