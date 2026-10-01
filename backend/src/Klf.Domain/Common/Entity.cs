namespace Klf.Domain.Common;

/// <summary>
/// Base class for every domain entity. Provides a time-ordered identifier (UUID v7) and UTC audit timestamps.
/// </summary>
public abstract class Entity
{
    /// <summary>Unique identifier, generated as a UUID v7 so new rows are naturally ordered by creation time.</summary>
    public Guid Id { get; protected init; } = Guid.CreateVersion7();

    /// <summary>When the entity was created, in UTC.</summary>
    public DateTime CreatedAt { get; protected init; } = DateTime.UtcNow;

    /// <summary>When the entity was last changed, in UTC; <see langword="null"/> if it was never updated.</summary>
    public DateTime? UpdatedAt { get; protected set; }

    /// <summary>Sets <see cref="UpdatedAt"/> to the current UTC time. Call it from every method that changes state.</summary>
    protected void MarkAsUpdated() => UpdatedAt = DateTime.UtcNow;
}
