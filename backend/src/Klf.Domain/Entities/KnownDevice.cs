using Klf.Domain.Common;

namespace Klf.Domain.Entities;

/// <summary>
/// A browser that already signed in to the panel with an account. The browser keeps a random token in an HttpOnly
/// cookie and only its SHA-256 hash is stored here. Signing in from a browser the account does not know triggers an
/// e-mail alert. Devices unused for a year are deleted.
/// </summary>
public sealed class KnownDevice : Entity
{
    /// <summary>Devices unused for this long are deleted (same lifetime as the cookie).</summary>
    public const int RetentionDays = 365;

    /// <summary>Registers a device on its first sign-in.</summary>
    /// <param name="userId">Account that signed in.</param>
    /// <param name="tokenHash">SHA-256 hash of the device token, in hexadecimal.</param>
    /// <param name="description">Browser and system, as shown to people (e.g. <c>Chrome no macOS</c>).</param>
    /// <param name="utcNow">Current time in UTC.</param>
    public KnownDevice(Guid userId, string tokenHash, string description, DateTime utcNow)
    {
        UserId = userId;
        TokenHash = tokenHash;
        Description = description;
        LastSeenAt = utcNow;
    }

    private KnownDevice()
    {
        TokenHash = string.Empty;
        Description = string.Empty;
    }

    /// <summary>Account that signed in from this device.</summary>
    public Guid UserId { get; private set; }

    /// <summary>SHA-256 hash of the device token, in hexadecimal.</summary>
    public string TokenHash { get; private set; }

    /// <summary>Browser and system of the last sign-in (e.g. <c>Chrome no macOS</c>).</summary>
    public string Description { get; private set; }

    /// <summary>Last sign-in from this device (UTC).</summary>
    public DateTime LastSeenAt { get; private set; }

    /// <summary>Records a new sign-in from this device.</summary>
    public void MarkSeen(string description, DateTime utcNow)
    {
        Description = description;
        LastSeenAt = utcNow;
    }

    /// <summary>Devices last seen before this moment are past the retention period.</summary>
    public static DateTime RetentionCutoff(DateTime utcNow) => utcNow.AddDays(-RetentionDays);
}
