namespace Klf.Application.DTOs.FeaturedQuote;

/// <summary>Creates or replaces the quote of the home page.</summary>
/// <param name="Text">The quote (up to 400 characters, plain text).</param>
/// <param name="Author">Who said it (up to 120 characters).</param>
/// <param name="Source">Where or when it was said (up to 150 characters); optional.</param>
/// <param name="IsPublished">Whether the home page shows it.</param>
public sealed record SaveFeaturedQuoteRequest(string Text, string Author, string? Source, bool IsPublished);

/// <summary>The quote of the home page, for the admin panel.</summary>
/// <param name="Text">The quote.</param>
/// <param name="Author">Who said it.</param>
/// <param name="Source">Where or when it was said.</param>
/// <param name="IsPublished">Whether the home page shows it.</param>
/// <param name="UpdatedAt">Last change.</param>
public sealed record FeaturedQuoteResponse(string Text, string Author, string? Source, bool IsPublished, DateTime UpdatedAt);

/// <summary>The quote as the home page shows it.</summary>
/// <param name="Text">The quote.</param>
/// <param name="Author">Who said it.</param>
/// <param name="Source">Where or when it was said.</param>
public sealed record PublicFeaturedQuoteResponse(string Text, string Author, string? Source);
