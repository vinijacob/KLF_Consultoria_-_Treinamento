using Klf.Application.DTOs.Audit;
using Klf.Application.Interfaces.Repositories;
using Klf.Application.Services.Audit;
using Klf.Application.Tests.Fakes;
using Klf.Domain.Entities;

namespace Klf.Application.Tests.Services;

public sealed class AuditLogServiceTests
{
    private readonly CapturingAuditLogRepository _repository = new();
    private readonly AuditLogService _service;

    public AuditLogServiceTests()
    {
        _service = new AuditLogService(_repository, new MutableTimeProvider(new DateTimeOffset(2026, 10, 14, 15, 0, 0, TimeSpan.Zero)));
    }

    [Fact]
    public async Task List_turns_manaus_days_into_a_utc_range_and_the_area_into_action_prefixes()
    {
        await _service.ListAsync(new AuditLogListRequest { From = new DateOnly(2026, 10, 1), To = new DateOnly(2026, 10, 1), Area = "team" }, TestContext.Current.CancellationToken);

        Assert.Equal(new DateTime(2026, 10, 1, 4, 0, 0, DateTimeKind.Utc), _repository.From);
        Assert.Equal(new DateTime(2026, 10, 2, 4, 0, 0, DateTimeKind.Utc), _repository.To);
        Assert.Equal(["user."], _repository.Prefixes);
    }

    [Fact]
    public async Task Purge_removes_entries_older_than_two_years()
    {
        await _service.PurgeExpiredAsync(TestContext.Current.CancellationToken);

        Assert.Equal(new DateTime(2024, 10, 14, 15, 0, 0, DateTimeKind.Utc), _repository.PurgeCutoff);
    }

    private sealed class CapturingAuditLogRepository : IAuditLogRepository
    {
        public DateTime? From { get; private set; }

        public DateTime? To { get; private set; }

        public IReadOnlyList<string> Prefixes { get; private set; } = [];

        public DateTime? PurgeCutoff { get; private set; }

        public Task<(IReadOnlyList<AuditLog> Items, int Total)> ListAsync(DateTime? fromUtc, DateTime? toUtc, Guid? actorId, IReadOnlyList<string> actionPrefixes, int skip, int take, CancellationToken cancellationToken)
        {
            (From, To, Prefixes) = (fromUtc, toUtc, actionPrefixes);
            return Task.FromResult<(IReadOnlyList<AuditLog>, int)>(([], 0));
        }

        public Task<int> DeleteOlderThanAsync(DateTime cutoff, CancellationToken cancellationToken)
        {
            PurgeCutoff = cutoff;
            return Task.FromResult(0);
        }

        public void Add(AuditLog entry)
        {
        }
    }
}
