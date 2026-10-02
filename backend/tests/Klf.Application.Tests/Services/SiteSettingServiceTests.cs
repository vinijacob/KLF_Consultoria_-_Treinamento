using System.Text.Json;

using Klf.Application.Services.Settings;
using Klf.Application.Tests.Fakes;
using Klf.Application.Validators.Settings;
using Klf.Domain.Common;
using Klf.Domain.Exceptions;

namespace Klf.Application.Tests.Services;

public sealed class SiteSettingServiceTests
{
    private readonly InMemorySiteSettingRepository _repository = new();
    private readonly SiteSettingService _service;

    public SiteSettingServiceTests()
    {
        _service = new SiteSettingService(
            _repository,
            _repository,
            new AboutSettingsValidator(),
            new ContactSettingsValidator(),
            new SocialSettingsValidator(),
            new SeoSettingsValidator());
    }

    [Fact]
    public async Task Upsert_creates_setting_with_camel_case_json_when_key_was_never_saved()
    {
        var value = await _service.UpsertAsync(SiteSettingKeys.Contact, Json("""{"Whatsapp":"5592999999999","email":"contato@klf.com.br"}"""), TestContext.Current.CancellationToken);

        var saved = Assert.Single(_repository.Settings);
        Assert.Equal(SiteSettingKeys.Contact, saved.Key);
        Assert.Equal("5592999999999", value.GetProperty("whatsapp").GetString());
        Assert.Contains("\"whatsapp\":\"5592999999999\"", saved.ValueJson, StringComparison.Ordinal);
        Assert.Equal(1, _repository.SaveCount);
    }

    [Fact]
    public async Task Upsert_replaces_value_and_keeps_a_single_row_when_key_already_exists()
    {
        await _service.UpsertAsync(SiteSettingKeys.Seo, Json("""{"title":"Antigo"}"""), TestContext.Current.CancellationToken);

        await _service.UpsertAsync(SiteSettingKeys.Seo, Json("""{"title":"Novo"}"""), TestContext.Current.CancellationToken);

        var saved = Assert.Single(_repository.Settings);
        Assert.Contains("Novo", saved.ValueJson, StringComparison.Ordinal);
        Assert.DoesNotContain("Antigo", saved.ValueJson, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Upsert_throws_not_found_when_key_does_not_exist()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.UpsertAsync("inventada", Json("{}"), TestContext.Current.CancellationToken));

        Assert.Empty(_repository.Settings);
    }

    [Fact]
    public async Task Upsert_rejects_unknown_field_when_value_has_a_typo()
    {
        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            _service.UpsertAsync(SiteSettingKeys.Contact, Json("""{"whatsap":"5592999999999"}"""), TestContext.Current.CancellationToken));

        Assert.Contains("whatsap", exception.Errors["value"][0], StringComparison.Ordinal);
        Assert.Empty(_repository.Settings);
    }

    [Theory]
    [InlineData("[]")]
    [InlineData("\"texto\"")]
    [InlineData("null")]
    [InlineData("""{"whatsapp":123}""")]
    public async Task Upsert_rejects_value_that_is_not_the_expected_object(string json)
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.UpsertAsync(SiteSettingKeys.Contact, Json(json), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Upsert_reports_every_invalid_field_in_portuguese()
    {
        var json = Json("""{"whatsapp":"abc","email":"nao-e-email","mapUrl":"http://inseguro.com"}""");

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            _service.UpsertAsync(SiteSettingKeys.Contact, json, TestContext.Current.CancellationToken));

        Assert.Equal(["Email", "MapUrl", "Whatsapp"], exception.Errors.Keys.Order(StringComparer.Ordinal));
        Assert.Equal(["Informe um e-mail válido."], exception.Errors["Email"]);
    }

    [Fact]
    public async Task Upsert_rejects_non_https_link_in_social_networks()
    {
        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            _service.UpsertAsync(SiteSettingKeys.Social, Json("""{"instagram":"javascript:alert(1)"}"""), TestContext.Current.CancellationToken));

        Assert.Contains("Instagram", exception.Errors.Keys);
    }

    [Fact]
    public async Task Upsert_rejects_about_with_too_many_values()
    {
        var values = string.Join(',', Enumerable.Range(0, 21).Select(i => $"\"v{i}\""));

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            _service.UpsertAsync(SiteSettingKeys.About, Json($$"""{"values":[{{values}}]}"""), TestContext.Current.CancellationToken));

        Assert.Contains("Values", exception.Errors.Keys);
    }

    [Fact]
    public async Task List_returns_only_saved_settings_keyed_by_name()
    {
        await _service.UpsertAsync(SiteSettingKeys.Seo, Json("""{"title":"KLF"}"""), TestContext.Current.CancellationToken);

        var all = await _service.ListAsync(TestContext.Current.CancellationToken);

        Assert.Equal([SiteSettingKeys.Seo], all.Keys);
        Assert.Equal("KLF", all[SiteSettingKeys.Seo].GetProperty("title").GetString());
    }

    [Fact]
    public async Task Get_throws_not_found_when_key_exists_but_was_never_saved()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.GetAsync(SiteSettingKeys.About, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Get_throws_not_found_when_key_does_not_exist()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.GetAsync("inventada", TestContext.Current.CancellationToken));
    }

    private static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);

        return document.RootElement.Clone();
    }
}
