using Klf.Domain.Entities;

namespace Klf.Application.Interfaces.Repositories;

/// <summary>Data access for the single <see cref="FeaturedQuote"/>.</summary>
public interface IFeaturedQuoteRepository
{
    /// <summary>Returns the quote, tracked for changes, or <see langword="null"/> while none was saved.</summary>
    Task<FeaturedQuote?> GetAsync(CancellationToken cancellationToken);

    /// <summary>Marks the quote to be inserted on the next <see cref="IUnitOfWork.SaveChangesAsync"/>.</summary>
    void Add(FeaturedQuote quote);
}
