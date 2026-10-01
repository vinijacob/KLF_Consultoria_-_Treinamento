using Klf.Application.DTOs.Career;
using Klf.Application.Validators.Career;
using Klf.Domain.Enums;

namespace Klf.Application.Tests.Validators;

public sealed class CareerEntryValidatorTests
{
    private static readonly DateOnly Start = new(2020, 3, 1);
    private readonly CreateCareerEntryRequestValidator _validator = new();

    [Fact]
    public void Request_is_valid_when_only_required_fields_are_filled()
    {
        var result = _validator.Validate(new CreateCareerEntryRequest(CareerEntryType.Education, "MBA", null, null, Start, null, 0));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Every_problem_is_reported_when_request_has_several_errors()
    {
        var request = new CreateCareerEntryRequest((CareerEntryType)99, "", new string('x', 151), null, Start, Start.AddDays(-1), -1);

        var errors = _validator.Validate(request).Errors.Select(e => e.PropertyName).ToHashSet();

        Assert.Equal(["DisplayOrder", "EndDate", "EntryType", "Institution", "Title"], errors.Order(StringComparer.Ordinal));
    }
}
