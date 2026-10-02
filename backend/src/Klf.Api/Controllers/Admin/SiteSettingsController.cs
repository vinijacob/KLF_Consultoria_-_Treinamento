using System.Text.Json;

using Klf.Api.Authorization;
using Klf.Application.Services.Settings;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Klf.Api.Controllers.Admin;

/// <summary>Edits the site settings in the admin panel.</summary>
[Route($"{RoutePrefix}/admin/settings")]
[Tags("Admin · Settings")]
[Authorize(Policy = Policies.ManageContent)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public sealed class SiteSettingsController(ISiteSettingService settingService) : ApiControllerBase
{
    private const int MaxBodyBytes = 100_000;

    /// <summary>Returns every configured setting.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyDictionary<string, JsonElement>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyDictionary<string, JsonElement>>> ListAsync(CancellationToken cancellationToken) =>
        Ok(await settingService.ListAsync(cancellationToken));

    /// <summary>Returns one setting. The key is <c>about</c>, <c>contact</c>, <c>social</c> or <c>seo</c>.</summary>
    [HttpGet("{key}")]
    [ProducesResponseType<JsonElement>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<JsonElement>> GetAsync(string key, CancellationToken cancellationToken) =>
        Ok(await settingService.GetAsync(key, cancellationToken));

    /// <summary>Creates or replaces one setting. The body is the JSON object of that key (see the DTOs of each setting).</summary>
    /// <remarks>
    /// Unknown fields and invalid values are rejected with 400. Always send the whole object: fields left out are cleared.
    /// </remarks>
    [HttpPut("{key}")]
    [RequestSizeLimit(MaxBodyBytes)]
    [ProducesResponseType<JsonElement>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<JsonElement>> UpsertAsync(string key, [FromBody] JsonElement value, CancellationToken cancellationToken) =>
        Ok(await settingService.UpsertAsync(key, value, cancellationToken));
}
