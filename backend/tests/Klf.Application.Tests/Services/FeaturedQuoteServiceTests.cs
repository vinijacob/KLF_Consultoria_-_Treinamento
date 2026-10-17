using Klf.Application.DTOs.FeaturedQuote;
using Klf.Application.Interfaces.Repositories;
using Klf.Application.Services.FeaturedQuote;
using Klf.Application.Tests.Fakes;
using Klf.Application.Validators.FeaturedQuote;
using Klf.Domain.Entities;

namespace Klf.Application.Tests.Services;

public sealed class FeaturedQuoteServiceTests
{
    private readonly InMemoryFeaturedQuoteRepository _repository = new();
    private readonly FeaturedQuoteService _service;

    public FeaturedQuoteServiceTests()
    {
        _service = new FeaturedQuoteService(_repository, _repository);
    }

    [Fact]
    public async Task Saving_twice_keeps_a_single_quote_with_the_last_text()
    {
        await _service.SaveAsync(new SaveFeaturedQuoteRequest(" Primeira ", " Kilciene ", " ", true), TestContext.Current.CancellationToken);
        var saved = await _service.SaveAsync(new SaveFeaturedQuoteRequest("Segunda", "Kilciene", "Palestra, 2025", true), TestContext.Current.CancellationToken);

        var quote = Assert.Single(_repository.Quotes);
        Assert.Equal("Segunda", quote.Text);
        Assert.Equal("Palestra, 2025", saved.Source);
    }

    [Fact]
    public async Task Public_quote_is_empty_until_saved_and_while_hidden()
    {
        Assert.Null(await _service.GetPublicAsync(TestContext.Current.CancellationToken));

        await _service.SaveAsync(new SaveFeaturedQuoteRequest("Atender bem é escutar.", "Kilciene", null, false), TestContext.Current.CancellationToken);
        Assert.Null(await _service.GetPublicAsync(TestContext.Current.CancellationToken));

        await _service.SaveAsync(new SaveFeaturedQuoteRequest("Atender bem é escutar.", "Kilciene", null, true), TestContext.Current.CancellationToken);
        Assert.Equal(new PublicFeaturedQuoteResponse("Atender bem é escutar.", "Kilciene", null), await _service.GetPublicAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void Quote_and_author_are_required()
    {
        var result = new SaveFeaturedQuoteRequestValidator().Validate(new SaveFeaturedQuoteRequest(" ", "", null, true));

        Assert.Contains(result.Errors, error => error.PropertyName == "Text");
        Assert.Contains(result.Errors, error => error.PropertyName == "Author");
    }

    private sealed class InMemoryFeaturedQuoteRepository : IFeaturedQuoteRepository, IUnitOfWork
    {
        public List<FeaturedQuote> Quotes { get; } = [];

        public Task<FeaturedQuote?> GetAsync(CancellationToken cancellationToken) => Task.FromResult(Quotes.FirstOrDefault());

        public void Add(FeaturedQuote quote) => Quotes.Add(quote);

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => Task.FromResult(1);
    }
}
