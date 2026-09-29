namespace Klf.Domain.Common;

public abstract class SoftDeletableEntity : Entity
{
    public DateTime? DeletedAt { get; private set; }

    public bool IsDeleted => DeletedAt is not null;

    public void SoftDelete()
    {
        if (IsDeleted)
        {
            return;
        }

        DeletedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

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
