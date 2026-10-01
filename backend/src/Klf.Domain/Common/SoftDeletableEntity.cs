namespace Klf.Domain.Common;

/// <summary>
/// Entity that is never physically removed from the database: deleting only stamps <see cref="DeletedAt"/>,
/// so the record can be restored later.
/// </summary>
public abstract class SoftDeletableEntity : Entity
{
    /// <summary>When the entity was soft-deleted, in UTC; <see langword="null"/> while it is active.</summary>
    public DateTime? DeletedAt { get; private set; }

    /// <summary>Whether the entity is currently soft-deleted.</summary>
    public bool IsDeleted => DeletedAt is not null;

    /// <summary>Marks the entity as deleted. Does nothing if it is already deleted.</summary>
    public void SoftDelete()
    {
        if (IsDeleted)
        {
            return;
        }

        DeletedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    /// <summary>Undoes a soft delete. Does nothing if the entity is not deleted.</summary>
    public void Restore()
    {
        if (!IsDeleted)
        {
            return;
        }

        DeletedAt = null;
        MarkAsUpdated();
    }
}
