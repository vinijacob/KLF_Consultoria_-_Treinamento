using Klf.Application.DTOs.FeaturedQuote;
using Klf.Application.Interfaces.Repositories;

namespace Klf.Application.Services.FeaturedQuote;

internal sealed class FeaturedQuoteService(IFeaturedQuoteRepository repository, IUnitOfWork unitOfWork) : IFeaturedQuoteService
{
    public async Task<PublicFeaturedQuoteResponse?> GetPublicAsync(CancellationToken cancellationToken) =>
        await repository.GetAsync(cancellationToken) is { IsPublished: true } quote
            ? new PublicFeaturedQuoteResponse(quote.Text, quote.Author, quote.Source)
            : null;

    public async Task<FeaturedQuoteResponse?> GetAsync(CancellationToken cancellationToken) =>
        await repository.GetAsync(cancellationToken) is { } quote ? ToResponse(quote) : null;

    public async Task<FeaturedQuoteResponse> SaveAsync(SaveFeaturedQuoteRequest request, CancellationToken cancellationToken)
    {
        var text = request.Text.Trim();
        var author = request.Author.Trim();
        var source = string.IsNullOrWhiteSpace(request.Source) ? null : request.Source.Trim();

        if (await repository.GetAsync(cancellationToken) is { } quote)
        {
            quote.Update(text, author, source, request.IsPublished);
        }
        else
        {
            quote = new Domain.Entities.FeaturedQuote(text, author, source, request.IsPublished);
            repository.Add(quote);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(quote);
    }

    private static FeaturedQuoteResponse ToResponse(Domain.Entities.FeaturedQuote quote) =>
        new(quote.Text, quote.Author, quote.Source, quote.IsPublished, quote.UpdatedAt ?? quote.CreatedAt);
}
