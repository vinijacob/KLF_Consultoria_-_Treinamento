using Klf.Application.DTOs.FeaturedQuote;

namespace Klf.Application.Services.FeaturedQuote;

/// <summary>The quote of Kilciene on the home page: one only, edited in the panel.</summary>
public interface IFeaturedQuoteService
{
    /// <summary>The published quote, or <see langword="null"/> when there is none or it is hidden.</summary>
    Task<PublicFeaturedQuoteResponse?> GetPublicAsync(CancellationToken cancellationToken);

    /// <summary>The quote for the panel, or <see langword="null"/> while none was saved.</summary>
    Task<FeaturedQuoteResponse?> GetAsync(CancellationToken cancellationToken);

    /// <summary>Creates the quote or replaces it.</summary>
    Task<FeaturedQuoteResponse> SaveAsync(SaveFeaturedQuoteRequest request, CancellationToken cancellationToken);
}
