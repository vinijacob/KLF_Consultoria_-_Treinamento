using Klf.Domain.Enums;

namespace Klf.Application.DTOs.Catalog;

/// <summary>Editable fields shared by the create and update requests, so both use the same validation rules.</summary>
public interface IServiceFields
{
    /// <summary>Name of the service.</summary>
    string Title { get; }

    /// <summary>URL-friendly identifier.</summary>
    string Slug { get; }

    /// <summary>Short description for cards.</summary>
    string? Summary { get; }

    /// <summary>Detail page text as HTML (sanitized by the server before saving).</summary>
    string ContentHtml { get; }

    /// <summary>Who the service is meant for.</summary>
    string Audience { get; }

    /// <summary>Duration, in hours.</summary>
    int WorkloadHours { get; }

    /// <summary>How the service is delivered.</summary>
    ServiceFormat Format { get; }

    /// <summary>Cover image id.</summary>
    Guid? CoverId { get; }

    /// <summary>Position in the public list.</summary>
    int DisplayOrder { get; }

    /// <summary>Whether the service is visible on the public site.</summary>
    bool IsActive { get; }
}
