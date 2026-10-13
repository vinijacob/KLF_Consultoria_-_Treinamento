namespace Klf.Domain.Enums;

/// <summary>Where a message sent through the site's contact form stands in the admin panel.</summary>
public enum ContactMessageStatus
{
    /// <summary>Nobody opened it yet.</summary>
    New,

    /// <summary>Opened in the panel, not answered yet.</summary>
    Read,

    /// <summary>KLF already replied (by e-mail, phone or WhatsApp).</summary>
    Answered,
}
