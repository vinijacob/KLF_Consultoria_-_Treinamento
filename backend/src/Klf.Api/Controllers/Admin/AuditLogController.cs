using Klf.Api.Authorization;
using Klf.Application.DTOs.Audit;
using Klf.Application.DTOs.Common;
using Klf.Application.Services.Audit;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Klf.Api.Controllers.Admin;

/// <summary>Audit log of the panel (RF38): who did what and when. Admin only; entries are kept 2 years.</summary>
[Route($"{RoutePrefix}/admin/audit-log")]
[Tags("Admin · Audit log")]
[Authorize(Policy = Policies.ViewAuditLog)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public sealed class AuditLogController(IAuditLogService auditLogService) : ApiControllerBase
{
    /// <summary>Lists entries newest first, filtered by period (Manaus days), person and area.</summary>
    [HttpGet]
    [ProducesResponseType<PagedResponse<AuditLogResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<AuditLogResponse>>> ListAsync([FromQuery] AuditLogListRequest request, CancellationToken cancellationToken) =>
        Ok(await auditLogService.ListAsync(request, cancellationToken));
}
