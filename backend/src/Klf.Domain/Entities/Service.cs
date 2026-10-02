using System.Diagnostics.CodeAnalysis;

using Klf.Domain.Common;
using Klf.Domain.Enums;

namespace Klf.Domain.Entities;

/// <summary>
/// A training or consulting service offered by KLF, shown on the public "Serviços" page and in its own detail page.
/// </summary>
public sealed class Service : SoftDeletableEntity
{
    /// <summary>Creates a service.</summary>
    /// <param name="title">Name of the service.</param>
    /// <param name="slug">Unique URL identifier.</param>
    /// <param name="summary">Short description for cards; optional.</param>
    /// <param name="contentHtml">Detail page text as sanitized HTML (objective, program...).</param>
    /// <param name="audience">Who the service is meant for.</param>
    /// <param name="workloadHours">Duration, in hours.</param>
    /// <param name="format">How the service is delivered.</param>
    /// <param name="coverId">Cover image id; optional.</param>
    /// <param name="displayOrder">Position in the public list; lower numbers come first.</param>
    /// <param name="isActive">Whether the service is visible on the public site.</param>
    public Service(
        string title,
        string slug,
        string? summary,
        string contentHtml,
        string audience,
        int workloadHours,
        ServiceFormat format,
        Guid? coverId,
        int displayOrder,
        bool isActive)
    {
        SetDetails(title, slug, summary, contentHtml, audience, workloadHours, format, coverId, displayOrder, isActive);
    }

    /// <summary>Name of the service.</summary>
    public string Title { get; private set; }

    /// <summary>Unique, URL-friendly identifier among non-deleted services (e.g. <c>lideranca-para-gestores</c>).</summary>
    public string Slug { get; private set; }

    /// <summary>Short description shown on cards; optional.</summary>
    public string? Summary { get; private set; }

    /// <summary>Detail page text as sanitized HTML (objective, program, methodology...).</summary>
    public string ContentHtml { get; private set; }

    /// <summary>Who the service is meant for (e.g. "Equipes de vendas do varejo").</summary>
    public string Audience { get; private set; }

    /// <summary>Duration of the service, in hours.</summary>
    public int WorkloadHours { get; private set; }

    /// <summary>How the service is delivered.</summary>
    public ServiceFormat Format { get; private set; }

    /// <summary>Cover image (will reference a <c>MediaAsset</c> once the images module exists); optional.</summary>
    public Guid? CoverId { get; private set; }

    /// <summary>Position in the public list; lower numbers come first.</summary>
    public int DisplayOrder { get; private set; }

    /// <summary>Whether the service is visible on the public site. Inactive services stay in the admin panel only.</summary>
    public bool IsActive { get; private set; }

    /// <summary>Replaces every editable field.</summary>
    /// <param name="title">Name of the service.</param>
    /// <param name="slug">Unique URL identifier.</param>
    /// <param name="summary">Short description for cards; optional.</param>
    /// <param name="contentHtml">Detail page text as sanitized HTML.</param>
    /// <param name="audience">Who the service is meant for.</param>
    /// <param name="workloadHours">Duration, in hours.</param>
    /// <param name="format">How the service is delivered.</param>
    /// <param name="coverId">Cover image id; optional.</param>
    /// <param name="displayOrder">Position in the public list.</param>
    /// <param name="isActive">Whether the service is visible on the public site.</param>
    public void Update(
        string title,
        string slug,
        string? summary,
        string contentHtml,
        string audience,
        int workloadHours,
        ServiceFormat format,
        Guid? coverId,
        int displayOrder,
        bool isActive)
    {
        SetDetails(title, slug, summary, contentHtml, audience, workloadHours, format, coverId, displayOrder, isActive);
    }

    [MemberNotNull(nameof(Title), nameof(Slug), nameof(ContentHtml), nameof(Audience))]
    private void SetDetails(
        string title,
        string slug,
        string? summary,
        string contentHtml,
        string audience,
        int workloadHours,
        ServiceFormat format,
        Guid? coverId,
        int displayOrder,
        bool isActive)
    {
        Title = title;
        Slug = slug;
        Summary = summary;
        ContentHtml = contentHtml;
        Audience = audience;
        WorkloadHours = workloadHours;
        Format = format;
        CoverId = coverId;
        DisplayOrder = displayOrder;
        IsActive = isActive;
    }
}
