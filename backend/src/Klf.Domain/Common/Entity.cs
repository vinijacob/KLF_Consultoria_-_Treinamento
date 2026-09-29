namespace Klf.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; protected init; } = Guid.CreateVersion7();

    public DateTime CreatedAt { get; protected init; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; protected set; }

    protected void MarkAsUpdated() => UpdatedAt = DateTime.UtcNow;
}
