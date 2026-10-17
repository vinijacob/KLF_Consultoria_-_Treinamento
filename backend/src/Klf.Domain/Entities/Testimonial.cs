using System.Linq.Expressions;

using Klf.Domain.Common;
using Klf.Domain.Exceptions;

namespace Klf.Domain.Entities;

/// <summary>
/// A named testimonial from a person trained by KLF. It only goes public with a recorded consent,
/// and revoking that consent hides it for good.
/// </summary>
public sealed class Testimonial : SoftDeletableEntity, IOrderedEntity
{
    /// <summary>Creates a testimonial.</summary>
    /// <param name="authorName">Who gave the testimonial.</param>
    /// <param name="authorRole">Job title of the author; optional.</param>
    /// <param name="companyName">Company or store of the author; optional.</param>
    /// <param name="quote">The testimonial text, plain text.</param>
    /// <param name="photoId">Photo of the author; optional, requires consent to use the image.</param>
    /// <param name="consentGivenAt">When the author signed the consent term; <see langword="null"/> if not yet signed.</param>
    /// <param name="consentCoversImage">Whether the consent also covers the use of the author's image.</param>
    /// <param name="isPublished">Whether the testimonial is shown on the public site.</param>
    /// <param name="displayOrder">Position in the public list; lower numbers come first.</param>
    /// <exception cref="ValidationException">A publishing rule is broken (see <see cref="Update"/>).</exception>
    public Testimonial(
        string authorName,
        string? authorRole,
        string? companyName,
        string quote,
        Guid? photoId,
        DateTime? consentGivenAt,
        bool consentCoversImage,
        bool isPublished,
        int displayOrder)
    {
        AuthorName = authorName;
        Quote = quote;
        Apply(authorName, authorRole, companyName, quote, photoId, consentGivenAt, consentCoversImage, isPublished);
        DisplayOrder = displayOrder;
    }

    /// <summary>Who gave the testimonial.</summary>
    public string AuthorName { get; private set; }

    /// <summary>Job title of the author; optional.</summary>
    public string? AuthorRole { get; private set; }

    /// <summary>Company or store of the author; optional.</summary>
    public string? CompanyName { get; private set; }

    /// <summary>The testimonial text, plain text.</summary>
    public string Quote { get; private set; }

    /// <summary>Photo of the author (will reference a <c>MediaAsset</c>); optional. Public only when <see cref="ConsentCoversImage"/>.</summary>
    public Guid? PhotoId { get; private set; }

    /// <summary>When the author signed the consent term (UTC); <see langword="null"/> if not yet signed.</summary>
    public DateTime? ConsentGivenAt { get; private set; }

    /// <summary>Whether the consent also covers the use of the author's image.</summary>
    public bool ConsentCoversImage { get; private set; }

    /// <summary>When the author revoked the consent (UTC); <see langword="null"/> while it stands.</summary>
    public DateTime? ConsentRevokedAt { get; private set; }

    /// <summary>Whether the testimonial is shown on the public site.</summary>
    public bool IsPublished { get; private set; }

    /// <summary>Position in the list; 0 comes first.</summary>
    public int DisplayOrder { get; private set; }

    /// <summary>Key of the signed consent term (PDF) in the private storage; <see langword="null"/> while none is attached.</summary>
    public string? ConsentDocumentKey { get; private set; }

    /// <summary>File name of the consent term as it was uploaded, to recognize it in the panel.</summary>
    public string? ConsentDocumentName { get; private set; }

    /// <summary>Size of the consent term in bytes.</summary>
    public long? ConsentDocumentSize { get; private set; }

    /// <summary>When the consent term was attached (UTC).</summary>
    public DateTime? ConsentDocumentUploadedAt { get; private set; }

    /// <summary>
    /// The rule for "visible on the public site": published, consent given and not revoked.
    /// Written as an expression so repositories can use it in database queries.
    /// </summary>
    public static Expression<Func<Testimonial, bool>> IsVisible { get; } =
        testimonial => testimonial.IsPublished && testimonial.ConsentGivenAt != null && testimonial.ConsentRevokedAt == null;

    /// <summary>Replaces every editable field except the position, which changes through <see cref="DisplayOrdering"/>.</summary>
    /// <param name="authorName">Who gave the testimonial.</param>
    /// <param name="authorRole">Job title of the author; optional.</param>
    /// <param name="companyName">Company or store of the author; optional.</param>
    /// <param name="quote">The testimonial text, plain text.</param>
    /// <param name="photoId">Photo of the author; optional.</param>
    /// <param name="consentGivenAt">When the author signed the consent term; optional.</param>
    /// <param name="consentCoversImage">Whether the consent also covers the use of the author's image.</param>
    /// <param name="isPublished">Whether the testimonial is shown on the public site.</param>
    /// <exception cref="ValidationException">
    /// Publishing without a consent date, publishing after the consent was revoked, or using a photo without image consent.
    /// </exception>
    public void Update(
        string authorName,
        string? authorRole,
        string? companyName,
        string quote,
        Guid? photoId,
        DateTime? consentGivenAt,
        bool consentCoversImage,
        bool isPublished)
    {
        Apply(authorName, authorRole, companyName, quote, photoId, consentGivenAt, consentCoversImage, isPublished);
    }

    /// <inheritdoc />
    public void MoveTo(int position) => DisplayOrder = position;

    /// <summary>
    /// Records that the author withdrew the consent: the testimonial is unpublished and can never be published again
    /// (a new consent means a new testimonial). Calling it twice keeps the first date.
    /// </summary>
    /// <param name="utcNow">Current time in UTC.</param>
    public void RevokeConsent(DateTime utcNow)
    {
        ConsentRevokedAt ??= utcNow;
        IsPublished = false;
    }

    /// <summary>Attaches the signed consent term, replacing the previous one.</summary>
    /// <param name="key">Key of the file in the private storage.</param>
    /// <param name="fileName">File name as it was uploaded.</param>
    /// <param name="size">File size in bytes.</param>
    /// <param name="utcNow">Current time in UTC.</param>
    /// <returns>The key of the replaced file, to be deleted from the storage, or <see langword="null"/>.</returns>
    public string? AttachConsentDocument(string key, string fileName, long size, DateTime utcNow)
    {
        var previous = ConsentDocumentKey;

        ConsentDocumentKey = key;
        ConsentDocumentName = fileName;
        ConsentDocumentSize = size;
        ConsentDocumentUploadedAt = utcNow;

        return previous;
    }

    /// <summary>Detaches the consent term.</summary>
    /// <returns>The key of the removed file, to be deleted from the storage, or <see langword="null"/> if there was none.</returns>
    public string? RemoveConsentDocument()
    {
        var previous = ConsentDocumentKey;

        ConsentDocumentKey = null;
        ConsentDocumentName = null;
        ConsentDocumentSize = null;
        ConsentDocumentUploadedAt = null;

        return previous;
    }

    private void Apply(
        string authorName,
        string? authorRole,
        string? companyName,
        string quote,
        Guid? photoId,
        DateTime? consentGivenAt,
        bool consentCoversImage,
        bool isPublished)
    {
        if (isPublished && consentGivenAt is null)
        {
            throw new ValidationException("IsPublished", "Só é possível publicar o depoimento depois de registrar o consentimento do autor.");
        }

        if (isPublished && ConsentRevokedAt is not null)
        {
            throw new ValidationException("IsPublished", "O autor revogou o consentimento. Cadastre um novo depoimento com novo consentimento.");
        }

        if (photoId is not null && !consentCoversImage)
        {
            throw new ValidationException("PhotoId", "Para usar a foto, o consentimento precisa cobrir o uso de imagem.");
        }

        AuthorName = authorName;
        AuthorRole = authorRole;
        CompanyName = companyName;
        Quote = quote;
        PhotoId = photoId;
        ConsentGivenAt = consentGivenAt;
        ConsentCoversImage = consentCoversImage;
        IsPublished = isPublished;
    }
}
