using Klf.Api.Authorization;
using Klf.Application.DTOs.Testimonials;
using Klf.Application.Services.Testimonials;
using Klf.Domain.Exceptions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Klf.Api.Controllers.Admin;

/// <summary>
/// The signed consent term (PDF) of a testimonial. The file lives in private storage and only leaves through these
/// routes, for people logged in to the panel.
/// </summary>
[Route($"{RoutePrefix}/admin/testimonials/{{id:guid}}/consent-document")]
[Tags("Admin · Testimonials")]
[Authorize(Policy = Policies.ManageContent)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public sealed class TestimonialConsentDocumentsController(ITestimonialConsentDocumentService documentService) : ApiControllerBase
{
    /// <summary>Downloads the signed consent term (PDF).</summary>
    [HttpGet]
    [Produces("application/pdf")]
    [ProducesResponseType<FileContentResult>(StatusCodes.Status200OK, "application/pdf")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<IActionResult> DownloadAsync(Guid id, CancellationToken cancellationToken)
    {
        var file = await documentService.DownloadAsync(id, cancellationToken);
        Response.Headers.CacheControl = "no-store";

        return File(file.Content, file.ContentType, file.FileName);
    }

    /// <summary>
    /// Attaches the signed consent term (multipart form with the field <c>file</c>), replacing the previous one.
    /// PDF up to 10 MB; the type is checked in the file content.
    /// </summary>
    [HttpPut]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(ITestimonialConsentDocumentService.MaxFileBytes + 1024 * 1024)]
    [ProducesResponseType<TestimonialResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TestimonialResponse>> AttachAsync(Guid id, [FromForm] UploadConsentDocumentForm form, CancellationToken cancellationToken)
    {
        if (form.File is null)
        {
            throw new ValidationException("File", "Envie o termo em PDF.");
        }

        await using var content = form.File.OpenReadStream();

        return Ok(await documentService.AttachAsync(
            id,
            new UploadConsentDocumentRequest(content, form.File.FileName, form.File.Length),
            cancellationToken));
    }

    /// <summary>Removes the signed consent term and deletes the file.</summary>
    [HttpDelete]
    [ProducesResponseType<TestimonialResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TestimonialResponse>> RemoveAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await documentService.RemoveAsync(id, cancellationToken));
}

/// <summary>Multipart form for <see cref="TestimonialConsentDocumentsController.AttachAsync"/>.</summary>
public sealed class UploadConsentDocumentForm
{
    /// <summary>The signed consent term (PDF, up to 10 MB).</summary>
    public IFormFile? File { get; init; }
}
