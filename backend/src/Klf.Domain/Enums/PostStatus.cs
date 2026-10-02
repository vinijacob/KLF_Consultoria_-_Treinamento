namespace Klf.Domain.Enums;

/// <summary>Publishing status of a post.</summary>
public enum PostStatus
{
    /// <summary>Saved in the admin panel only; never shown on the public site.</summary>
    Draft,

    /// <summary>Set to go live at a future date; it appears on the public site once that date arrives.</summary>
    Scheduled,

    /// <summary>Live on the public site.</summary>
    Published,
}
