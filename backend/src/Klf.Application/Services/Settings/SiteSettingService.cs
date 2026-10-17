using System.Text.Json;

using FluentValidation;

using Klf.Application.DTOs.Settings;
using Klf.Application.Interfaces.Repositories;
using Klf.Application.Services.Media;
using Klf.Application.Validators.Settings;
using Klf.Domain.Common;
using Klf.Domain.Entities;
using Klf.Domain.Exceptions;

using ValidationException = Klf.Domain.Exceptions.ValidationException;

namespace Klf.Application.Services.Settings;

internal sealed class SiteSettingService(
    ISiteSettingRepository repository,
    IMediaAssetRepository media,
    IUnitOfWork unitOfWork,
    IValidator<AboutSettings> aboutValidator,
    IValidator<ContactSettings> contactValidator,
    IValidator<SocialSettings> socialValidator,
    IValidator<SeoSettings> seoValidator,
    IValidator<LegalSettings> legalValidator,
    TimeProvider timeProvider) : ISiteSettingService
{
    private static readonly JsonSerializerOptions StrictJson = new(JsonSerializerDefaults.Web)
    {
        UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow,
    };

    public async Task<IReadOnlyDictionary<string, JsonElement>> ListAsync(CancellationToken cancellationToken)
    {
        var settings = await repository.ListAsync(cancellationToken);

        return settings.ToDictionary(setting => setting.Key, setting => ToElement(setting.ValueJson));
    }

    public async Task<JsonElement> GetAsync(string key, CancellationToken cancellationToken)
    {
        EnsureKeyExists(key);

        var setting = await repository.GetByKeyAsync(key, cancellationToken)
            ?? throw new NotFoundException($"A configuração '{key}' ainda não foi preenchida.");

        return ToElement(setting.ValueJson);
    }

    public async Task<JsonElement> UpsertAsync(string key, JsonElement value, CancellationToken cancellationToken)
    {
        string valueJson;

        switch (key)
        {
            case SiteSettingKeys.About:
                var about = Parse(key, value, aboutValidator);
                foreach (var portraitId in about.PortraitIds ?? [])
                {
                    await MediaReference.EnsureExistsAsync(media, portraitId, nameof(AboutSettings.PortraitIds), cancellationToken);
                }

                valueJson = Serialize(about);
                break;
            case SiteSettingKeys.Contact:
                valueJson = Serialize(Parse(key, value, contactValidator));
                break;
            case SiteSettingKeys.Social:
                var social = Parse(key, value, socialValidator);
                valueJson = Serialize(new SocialSettings([.. (social.Links ?? []).Select(link => new SocialLink(link.Name.Trim(), link.Url.Trim()))]));
                break;
            case SiteSettingKeys.Seo:
                var seo = Parse(key, value, seoValidator);
                await MediaReference.EnsureExistsAsync(media, seo.ShareImageId, nameof(SeoSettings.ShareImageId), cancellationToken);
                valueJson = Serialize(seo);
                break;
            case SiteSettingKeys.Legal:
                var legal = Parse(key, value, legalValidator);
                valueJson = Serialize(legal with
                {
                    Cnpj = string.IsNullOrWhiteSpace(legal.Cnpj) ? null : Cnpj.Format(legal.Cnpj),
                    UpdatedOn = AppTimeZone.Today(timeProvider),
                });
                break;
            default:
                throw UnknownKey(key);
        }

        var setting = await repository.GetByKeyAsync(key, cancellationToken);

        if (setting is null)
        {
            repository.Add(new SiteSetting(key, valueJson));
        }
        else
        {
            setting.Update(valueJson);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToElement(valueJson);
    }

    private static void EnsureKeyExists(string key)
    {
        if (!SiteSettingKeys.All.Contains(key))
        {
            throw UnknownKey(key);
        }
    }

    private static NotFoundException UnknownKey(string key) =>
        new($"A configuração '{key}' não existe. Use: {string.Join(", ", SiteSettingKeys.All)}.");

    private static JsonElement ToElement(string json)
    {
        using var document = JsonDocument.Parse(json);

        return document.RootElement.Clone();
    }

    private static string Serialize<T>(T value) => JsonSerializer.Serialize(value, StrictJson);

    private static T Parse<T>(string key, JsonElement value, IValidator<T> validator)
        where T : class
    {
        T? typed;

        try
        {
            typed = value.Deserialize<T>(StrictJson);
        }
        catch (JsonException exception)
        {
            var where = string.IsNullOrEmpty(exception.Path) ? string.Empty : $" (em {exception.Path})";

            throw new ValidationException("value", $"O valor enviado não corresponde ao formato de '{key}'{where}. Confira os nomes e os tipos dos campos.");
        }

        if (typed is null)
        {
            throw new ValidationException("value", $"Envie um objeto JSON para '{key}'.");
        }

        var result = validator.Validate(typed);

        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray()));
        }

        return typed;
    }
}
