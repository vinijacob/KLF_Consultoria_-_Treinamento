using Klf.Application.DTOs.Settings;
using Klf.Application.Validators.Settings;

namespace Klf.Application.Tests.Validators;

public sealed class LegalSettingsValidatorTests
{
    private readonly LegalSettingsValidator _validator = new();

    private static LegalSettings Empty() => new(null, null, null, null, null, null);

    [Fact]
    public void Settings_are_valid_when_every_field_is_empty()
    {
        Assert.True(_validator.Validate(Empty()).IsValid);
    }

    [Theory]
    [InlineData("11.222.333/0001-81")]
    [InlineData("11222333000181")]
    [InlineData("12.ABC.345/01DE-35")]
    [InlineData("12abc34501de35")]
    public void Cnpj_is_accepted_when_check_digits_match(string cnpj)
    {
        Assert.True(_validator.Validate(Empty() with { Cnpj = cnpj }).IsValid);
    }

    [Theory]
    [InlineData("11.222.333/0001-82")]
    [InlineData("12.ABC.345/01DE-36")]
    [InlineData("00.000.000/0000-00")]
    [InlineData("1122233300018")]
    [InlineData("12.ABC.345/01DE-3A")]
    [InlineData("11_222_333_0001_81")]
    public void Cnpj_is_rejected_when_it_is_not_a_valid_number(string cnpj)
    {
        var result = _validator.Validate(Empty() with { Cnpj = cnpj });

        Assert.Contains(result.Errors, e => e.PropertyName == "Cnpj");
    }

    [Theory]
    [InlineData("Manaus/AM", true)]
    [InlineData("São Paulo/SP", true)]
    [InlineData("Manaus", false)]
    [InlineData("Manaus/am", false)]
    public void Forum_city_needs_city_and_state(string forum, bool valid)
    {
        Assert.Equal(valid, _validator.Validate(Empty() with { ForumCity = forum }).IsValid);
    }

    [Fact]
    public void Dpo_email_is_rejected_when_it_is_not_an_email()
    {
        var result = _validator.Validate(Empty() with { DpoEmail = "nao-e-email" });

        Assert.Contains(result.Errors, e => e.PropertyName == "DpoEmail");
    }
}
