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
    private readonly InMemoryMediaAssetRepository _media = new();
    private readonly MutableTimeProvider _clock = new(new DateTimeOffset(2026, 10, 13, 2, 0, 0, TimeSpan.Zero));
    private readonly SiteSettingService _service;

    public SiteSettingServiceTests()
    {
        _service = new SiteSettingService(
            _repository,
            _media,
            _repository,
            new AboutSettingsValidator(),
            new ContactSettingsValidator(),
            new SocialSettingsValidator(),
            new SeoSettingsValidator(),
            new LegalSettingsValidator(),
            _clock);
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

    [Fact]
    public async Task Upsert_about_saves_portrait_when_image_exists_and_rejects_unknown_image()
    {
        var portrait = _media.Seed();
        var token = TestContext.Current.CancellationToken;

        var saved = await _service.UpsertAsync(SiteSettingKeys.About, Json($$"""{"mission":"Missão","portraitId":"{{portrait.Id}}"}"""), token);
        var missing = await Assert.ThrowsAsync<ValidationException>(() =>
            _service.UpsertAsync(SiteSettingKeys.About, Json($$"""{"portraitId":"{{Guid.CreateVersion7()}}"}"""), token));

        Assert.Equal(portrait.Id, saved.GetProperty("portraitId").GetGuid());
        Assert.Contains("PortraitId", missing.Errors.Keys);
    }

    [Fact]
    public async Task Upsert_seo_rejects_share_image_that_does_not_exist()
    {
        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            _service.UpsertAsync(SiteSettingKeys.Seo, Json($$"""{"shareImageId":"{{Guid.CreateVersion7()}}"}"""), TestContext.Current.CancellationToken));

        Assert.Contains("ShareImageId", exception.Errors.Keys);
    }

    [Fact]
    public async Task Upsert_legal_formats_cnpj_and_stamps_the_date_in_manaus_time()
    {
        var saved = await _service.UpsertAsync(SiteSettingKeys.Legal, Json("""{"companyName":"KLF Ltda","cnpj":"12abc34501de35"}"""), TestContext.Current.CancellationToken);

        Assert.Equal("12.ABC.345/01DE-35", saved.GetProperty("cnpj").GetString());
        Assert.Equal("2026-10-12", saved.GetProperty("updatedOn").GetString());
    }

    [Fact]
    public async Task Upsert_legal_ignores_the_date_sent_by_the_client()
    {
        var saved = await _service.UpsertAsync(SiteSettingKeys.Legal, Json("""{"dpoEmail":"dados@klf.com.br","updatedOn":"2020-01-01"}"""), TestContext.Current.CancellationToken);

        Assert.Equal("2026-10-12", saved.GetProperty("updatedOn").GetString());
        Assert.Equal(JsonValueKind.Null, saved.GetProperty("cnpj").ValueKind);
    }

    [Fact]
    public async Task Upsert_legal_rejects_cnpj_with_wrong_check_digits()
    {
        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            _service.UpsertAsync(SiteSettingKeys.Legal, Json("""{"cnpj":"11.222.333/0001-82"}"""), TestContext.Current.CancellationToken));

        Assert.Contains("Cnpj", exception.Errors.Keys);
        Assert.Empty(_repository.Settings);
    }

    private static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);

        return document.RootElement.Clone();
    }
}
