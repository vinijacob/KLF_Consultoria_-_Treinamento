using Klf.Application.DTOs.Catalog;
using Klf.Application.Validators.Catalog;
using Klf.Domain.Enums;

namespace Klf.Application.Tests.Validators;

public sealed class ServiceValidatorTests
{
    private readonly CreateServiceRequestValidator _validator = new();

    [Fact]
    public void Request_is_valid_when_only_required_fields_are_filled()
    {
        Assert.True(_validator.Validate(Valid()).IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(2001)]
    public void Workload_is_rejected_when_out_of_range(int hours)
    {
        var result = _validator.Validate(Valid() with { WorkloadHours = hours });

        Assert.Contains(result.Errors, e => e.PropertyName == "WorkloadHours");
    }

    [Theory]
    [InlineData("Com Espaço")]
    [InlineData("MAIUSCULA")]
    [InlineData("-hifen")]
    public void Slug_is_rejected_when_it_is_not_url_friendly(string slug)
    {
        var result = _validator.Validate(Valid() with { Slug = slug });

        Assert.Contains(result.Errors, e => e.PropertyName == "Slug");
    }

    [Fact]
    public void Every_problem_is_reported_when_request_has_several_errors()
    {
        var request = new CreateServiceRequest("", "", new string('x', 501), "", "", 0, (ServiceFormat)99, Guid.Empty, -1, true);

        var fields = _validator.Validate(request).Errors.Select(e => e.PropertyName).ToHashSet();

        Assert.Equal(
            ["Audience", "ContentHtml", "CoverId", "DisplayOrder", "Format", "Slug", "Summary", "Title", "WorkloadHours"],
            fields.Order(StringComparer.Ordinal));
    }

    private static CreateServiceRequest Valid() =>
        new("Liderança para gestores", "lideranca-para-gestores", null, "<p>x</p>", "Gestores", 16, ServiceFormat.InCompany, null, 0, true);
}
