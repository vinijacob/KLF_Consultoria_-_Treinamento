using Klf.Domain.Common;

namespace Klf.Domain.Tests.Common;

public sealed class AppTimeZoneTests
{
    [Fact]
    public void Local_time_is_four_hours_behind_when_converting_from_utc()
    {
        var utc = new DateTime(2026, 9, 30, 12, 0, 0, DateTimeKind.Utc);

        var local = AppTimeZone.ToLocal(utc);

        Assert.Equal(new DateTime(2026, 9, 30, 8, 0, 0), local);
    }

    [Fact]
    public void Utc_is_four_hours_ahead_when_converting_from_local()
    {
        var local = new DateTime(2026, 9, 30, 20, 30, 0);

        var utc = AppTimeZone.ToUtc(local);

        Assert.Equal(new DateTime(2026, 10, 1, 0, 30, 0, DateTimeKind.Utc), utc);
        Assert.Equal(DateTimeKind.Utc, utc.Kind);
    }

    [Fact]
    public void Conversion_fails_when_date_is_not_utc()
    {
        Assert.Throws<ArgumentException>(() => AppTimeZone.ToLocal(new DateTime(2026, 9, 30, 12, 0, 0, DateTimeKind.Local)));
    }

    [Fact]
    public void Today_is_previous_day_when_utc_already_turned_but_manaus_did_not()
    {
        var clock = new FixedTimeProvider(new DateTimeOffset(2026, 10, 1, 2, 0, 0, TimeSpan.Zero));

        Assert.Equal(new DateOnly(2026, 9, 30), AppTimeZone.Today(clock));
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
