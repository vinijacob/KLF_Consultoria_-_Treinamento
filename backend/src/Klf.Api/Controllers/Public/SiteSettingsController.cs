using System.Text.Json;

using Klf.Application.Services.Settings;

using Microsoft.AspNetCore.Mvc;

namespace Klf.Api.Controllers.Public;

/// <summary>Site settings shown on the public site: institutional texts, contact, social networks and default SEO.</summary>
[Route($"{RoutePrefix}/public/settings")]
[Tags("Settings")]
public sealed class SiteSettingsController(ISiteSettingService settingService) : ApiControllerBase
{
    /// <summary>Returns every configured setting, grouped by name: <c>{ "about": {...}, "contact": {...}, "social": {...}, "seo": {...} }</c>.</summary>
    /// <remarks>Settings that were never saved are absent; the frontend must handle missing keys.</remarks>
    [HttpGet]
    [ProducesResponseType<IReadOnlyDictionary<string, JsonElement>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyDictionary<string, JsonElement>>> ListAsync(CancellationToken cancellationToken) =>
        Ok(await settingService.ListAsync(cancellationToken));
}
