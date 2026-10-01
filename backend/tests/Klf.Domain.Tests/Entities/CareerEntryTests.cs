using Klf.Domain.Entities;
using Klf.Domain.Enums;
using Klf.Domain.Exceptions;

namespace Klf.Domain.Tests.Entities;

public sealed class CareerEntryTests
{
    private static readonly DateOnly Start = new(2020, 3, 1);

    [Fact]
    public void Entry_is_ongoing_when_end_date_is_empty()
    {
        var entry = new CareerEntry(CareerEntryType.Experience, "Consultora", "KLF", null, Start, null, 0);

        Assert.True(entry.IsOngoing);
    }

    [Fact]
    public void Creation_fails_when_end_date_is_before_start_date()
    {
        var exception = Assert.Throws<ValidationException>(() =>
            new CareerEntry(CareerEntryType.Education, "MBA", "FGV", null, Start, Start.AddDays(-1), 0));

        Assert.True(exception.Errors.ContainsKey(nameof(CareerEntry.EndDate)));
    }

    [Fact]
    public void Update_fails_and_keeps_old_values_when_end_date_is_before_start_date()
    {
        var entry = new CareerEntry(CareerEntryType.Education, "MBA", "FGV", null, Start, null, 0);

        Assert.Throws<ValidationException>(() =>
            entry.Update(CareerEntryType.Education, "Outro", "FGV", null, Start, Start.AddDays(-1), 0));

        Assert.Equal("MBA", entry.Title);
    }
}
