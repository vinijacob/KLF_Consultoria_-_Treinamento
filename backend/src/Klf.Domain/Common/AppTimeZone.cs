namespace Klf.Domain.Common;

/// <summary>
/// The business time zone (America/Manaus). Dates are always stored in UTC; convert with this class only at the edges
/// (showing dates, exports, and rules that depend on the local calendar day).
/// </summary>
public static class AppTimeZone
{
    /// <summary>IANA identifier of the business time zone.</summary>
    public const string Id = "America/Manaus";

    /// <summary>The business time zone.</summary>
    public static TimeZoneInfo Info { get; } = TimeZoneInfo.FindSystemTimeZoneById(Id);

    /// <summary>Converts a UTC date/time to Manaus local time.</summary>
    /// <exception cref="ArgumentException"><paramref name="utc"/> is not <see cref="DateTimeKind.Utc"/>.</exception>
    public static DateTime ToLocal(DateTime utc) => utc.Kind == DateTimeKind.Utc
        ? TimeZoneInfo.ConvertTimeFromUtc(utc, Info)
        : throw new ArgumentException("A data precisa estar em UTC.", nameof(utc));

    /// <summary>Converts a Manaus local date/time (as typed by a user) to UTC for storage.</summary>
    public static DateTime ToUtc(DateTime local) =>
        TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(local, DateTimeKind.Unspecified), Info);

    /// <summary>The current calendar day in Manaus.</summary>
    public static DateOnly Today(TimeProvider timeProvider) =>
        DateOnly.FromDateTime(ToLocal(timeProvider.GetUtcNow().UtcDateTime));
}
