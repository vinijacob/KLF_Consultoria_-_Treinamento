namespace Klf.Domain.Common;

/// <summary>
/// An entity shown in a list ordered by hand in the panel. Positions start at 0 (first) and have no gaps or repeats;
/// <see cref="DisplayOrdering"/> keeps them that way.
/// </summary>
public interface IOrderedEntity
{
    /// <summary>Identifier, used to find the entity in its list.</summary>
    Guid Id { get; }

    /// <summary>Position in the list; 0 comes first.</summary>
    int DisplayOrder { get; }

    /// <summary>Sets the position. Call it only through <see cref="DisplayOrdering"/>, so the rest of the list follows.</summary>
    void MoveTo(int position);
}
