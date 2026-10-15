using Klf.Application.Interfaces.Audit;

namespace Klf.Api.Tests.Fakes;

public sealed class FakeAuditTrail : IAuditTrail
{
    public List<AuditEntry> Entries { get; } = [];

    public Task RecordAsync(AuditEntry entry, CancellationToken cancellationToken)
    {
        Entries.Add(entry);
        return Task.CompletedTask;
    }
}
