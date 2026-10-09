using Klf.Application.Interfaces.Repositories;
using Klf.Domain.Entities;

namespace Klf.Api.Tests.Fakes;

public sealed class InMemoryFeedbackStore : IFeedbackFormRepository, IFeedbackSessionRepository, IFeedbackResponseRepository, IUnitOfWork
{
    public List<FeedbackForm> Forms { get; } = [];

    public List<FeedbackSession> Sessions { get; } = [];

    public List<FeedbackResponse> Responses { get; } = [];

    public int SaveCount { get; private set; }

    Task<IReadOnlyList<FeedbackForm>> IFeedbackFormRepository.ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<FeedbackForm>>([.. Forms.OrderBy(f => f.Title)]);

    Task<FeedbackForm?> IFeedbackFormRepository.GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Forms.SingleOrDefault(f => f.Id == id));

    public void Add(FeedbackForm form) => Forms.Add(form);

    public void Remove(FeedbackForm form) => Forms.Remove(form);

    public Task<(IReadOnlyList<FeedbackSession> Items, int TotalItems)> ListAsync(FeedbackSessionFilter filter, int skip, int take, CancellationToken cancellationToken)
    {
        var all = Filter(filter).ToList();
        return Task.FromResult<(IReadOnlyList<FeedbackSession>, int)>(([.. all.Skip(skip).Take(take)], all.Count));
    }

    public Task<IReadOnlyList<FeedbackSession>> ListAllAsync(FeedbackSessionFilter filter, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<FeedbackSession>>([.. Filter(filter)]);

    Task<FeedbackSession?> IFeedbackSessionRepository.GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Sessions.SingleOrDefault(s => s.Id == id));

    public Task<FeedbackSession?> GetByPublicCodeAsync(string publicCode, CancellationToken cancellationToken) =>
        Task.FromResult(Sessions.SingleOrDefault(s => s.PublicCode == publicCode));

    public void Add(FeedbackSession session) => Sessions.Add(session);

    public void Remove(FeedbackSession session) => Sessions.Remove(session);

    public Task<int> CountBySessionAsync(Guid sessionId, CancellationToken cancellationToken) =>
        Task.FromResult(Responses.Count(r => r.SessionId == sessionId));

    public Task<IReadOnlyDictionary<Guid, int>> CountBySessionsAsync(IReadOnlyCollection<Guid> sessionIds, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyDictionary<Guid, int>>(Responses
            .Where(r => sessionIds.Contains(r.SessionId))
            .GroupBy(r => r.SessionId)
            .ToDictionary(g => g.Key, g => g.Count()));

    public Task<IReadOnlyList<FeedbackResponse>> ListBySessionsAsync(IReadOnlyCollection<Guid> sessionIds, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<FeedbackResponse>>([.. Responses.Where(r => sessionIds.Contains(r.SessionId))]);

    public void Add(FeedbackResponse response) => Responses.Add(response);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => Task.FromResult(++SaveCount);

    public FeedbackSession SeedSession(Guid ownerId, DateTime opensAt, DateTime closesAt, int? maxResponses = null, string title = "Loja Centro")
    {
        var session = new FeedbackSession(ownerId, title, null, "Avaliação", null, FeedbackSamples.Definition(), opensAt, closesAt, maxResponses, null, null);
        Sessions.Add(session);
        return session;
    }

    public void SeedResponses(FeedbackSession session, params int[] npsScores)
    {
        foreach (var nps in npsScores)
        {
            Responses.Add(new FeedbackResponse(session.Id, new DateOnly(2026, 10, 7), FeedbackSamples.ValidAnswers(nps, $"Texto {nps}")));
        }
    }

    private IEnumerable<FeedbackSession> Filter(FeedbackSessionFilter filter) => Sessions
        .Where(s => filter.ClientId is null || s.ClientId == filter.ClientId)
        .Where(s => filter.ServiceId is null || s.ServiceId == filter.ServiceId)
        .Where(s => filter.OpensFrom is null || s.OpensAt >= filter.OpensFrom)
        .Where(s => filter.OpensBefore is null || s.OpensAt < filter.OpensBefore)
        .Where(s => filter.Search is null || s.Title.Contains(filter.Search, StringComparison.OrdinalIgnoreCase))
        .OrderByDescending(s => s.OpensAt);
}

public sealed class FakePosterRenderer : Klf.Application.Interfaces.Documents.IFeedbackPosterRenderer
{
    public Klf.Application.Interfaces.Documents.FeedbackPoster? LastPoster { get; private set; }

    public byte[] RenderQrCodePng(Uri url) => [0x89, 0x50, 0x4E, 0x47];

    public byte[] RenderPosterPdf(Klf.Application.Interfaces.Documents.FeedbackPoster poster)
    {
        LastPoster = poster;
        return "%PDF"u8.ToArray();
    }
}
