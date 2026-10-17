using Klf.Application.Interfaces.Repositories;
using Klf.Domain.Entities;
using Klf.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Klf.Infrastructure.Repositories;

internal sealed class FeaturedQuoteRepository(AppDbContext context) : IFeaturedQuoteRepository
{
    public Task<FeaturedQuote?> GetAsync(CancellationToken cancellationToken) =>
        context.FeaturedQuotes.OrderBy(quote => quote.CreatedAt).FirstOrDefaultAsync(cancellationToken);

    public void Add(FeaturedQuote quote) => context.FeaturedQuotes.Add(quote);
}
