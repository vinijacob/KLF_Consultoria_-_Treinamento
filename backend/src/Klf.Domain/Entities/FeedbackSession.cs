using System.Buffers.Text;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

using Klf.Domain.Common;
using Klf.Domain.Enums;
using Klf.Domain.Exceptions;

namespace Klf.Domain.Entities;

/// <summary>
/// A class (turma) being evaluated: trainees scan its QR Code and answer anonymously while it is open.
/// Holds a frozen copy of the form, so the questions never change under answers already given.
/// </summary>
public sealed class FeedbackSession : SoftDeletableEntity
{
    /// <summary>Creates a session with a new random public code.</summary>
    /// <param name="ownerId">User who manages the session (an instructor sees only their own sessions).</param>
    /// <param name="title">Internal and public name, e.g. "Loja Centro — Atendimento (manhã)".</param>
    /// <param name="formId">Template the form was copied from; optional.</param>
    /// <param name="formTitle">Title respondents see.</param>
    /// <param name="formDescription">Text shown at the top of the form; optional.</param>
    /// <param name="definition">Frozen copy of the questions.</param>
    /// <param name="opensAt">When responses start being accepted (UTC).</param>
    /// <param name="closesAt">When responses stop being accepted (UTC).</param>
    /// <param name="maxResponses">Stops accepting responses after this many; optional.</param>
    /// <param name="clientId">Company or store trained; optional.</param>
    /// <param name="serviceId">Training given; optional.</param>
    /// <exception cref="ValidationException">Invalid definition or period.</exception>
    public FeedbackSession(
        Guid ownerId,
        string title,
        Guid? formId,
        string formTitle,
        string? formDescription,
        FormDefinition definition,
        DateTime opensAt,
        DateTime closesAt,
        int? maxResponses,
        Guid? clientId,
        Guid? serviceId)
    {
        OwnerId = ownerId;
        PublicCode = NewPublicCode();
        Update(title, opensAt, closesAt, maxResponses, clientId, serviceId);
        SetForm(formId, formTitle, formDescription, definition);
    }

    private FeedbackSession()
    {
        Title = null!;
        PublicCode = null!;
        FormTitle = null!;
        Definition = null!;
    }

    /// <summary>Length of <see cref="PublicCode"/>: 16 random bytes (128 bits) in Base64Url.</summary>
    public const int PublicCodeLength = 22;

    /// <summary>User who manages the session.</summary>
    public Guid OwnerId { get; private set; }

    /// <summary>Internal and public name of the session.</summary>
    public string Title { get; private set; }

    /// <summary>Random, unguessable code used in the public address and the QR Code.</summary>
    public string PublicCode { get; private set; }

    /// <summary>Template the form was copied from; optional.</summary>
    public Guid? FormId { get; private set; }

    /// <summary>Title respondents see.</summary>
    public string FormTitle { get; private set; }

    /// <summary>Text shown at the top of the form; optional.</summary>
    public string? FormDescription { get; private set; }

    /// <summary>Frozen copy of the questions.</summary>
    public FormDefinition Definition { get; private set; }

    /// <summary>When responses start being accepted (UTC).</summary>
    public DateTime OpensAt { get; private set; }

    /// <summary>When responses stop being accepted (UTC).</summary>
    public DateTime ClosesAt { get; private set; }

    /// <summary>Stops accepting responses after this many; optional.</summary>
    public int? MaxResponses { get; private set; }

    /// <summary>When the session was closed by hand before <see cref="ClosesAt"/> (UTC); optional.</summary>
    public DateTime? ClosedAt { get; private set; }

    /// <summary>Company or store trained; optional.</summary>
    public Guid? ClientId { get; private set; }

    /// <summary>Training given; optional.</summary>
    public Guid? ServiceId { get; private set; }

    /// <summary>Whether the session accepts responses now.</summary>
    /// <param name="utcNow">Current time in UTC.</param>
    /// <param name="responseCount">Responses received so far.</param>
    public FeedbackSessionStatus GetStatus(DateTime utcNow, int responseCount)
    {
        if (ClosedAt is not null || utcNow >= ClosesAt || (MaxResponses is { } max && responseCount >= max))
        {
            return FeedbackSessionStatus.Closed;
        }

        return utcNow < OpensAt ? FeedbackSessionStatus.Scheduled : FeedbackSessionStatus.Open;
    }

    /// <summary>Replaces the scheduling and the links. Does not reopen a session closed by hand (use <see cref="Reopen"/>).</summary>
    /// <param name="title">Name of the session.</param>
    /// <param name="opensAt">When responses start being accepted (UTC).</param>
    /// <param name="closesAt">When responses stop being accepted (UTC).</param>
    /// <param name="maxResponses">Response limit; optional.</param>
    /// <param name="clientId">Company or store trained; optional.</param>
    /// <param name="serviceId">Training given; optional.</param>
    /// <exception cref="ValidationException">The session closes before it opens.</exception>
    [MemberNotNull(nameof(Title))]
    public void Update(string title, DateTime opensAt, DateTime closesAt, int? maxResponses, Guid? clientId, Guid? serviceId)
    {
        if (closesAt <= opensAt)
        {
            throw new ValidationException("ClosesAt", "O encerramento precisa ser depois da abertura.");
        }

        Title = title;
        OpensAt = opensAt;
        ClosesAt = closesAt;
        MaxResponses = maxResponses;
        ClientId = clientId;
        ServiceId = serviceId;
    }

    /// <summary>Replaces the questions. Only allowed while nobody has answered, so results always match the questions.</summary>
    /// <param name="formId">Template the form was copied from; optional.</param>
    /// <param name="formTitle">Title respondents see.</param>
    /// <param name="formDescription">Text shown at the top of the form; optional.</param>
    /// <param name="definition">New questions.</param>
    /// <param name="responseCount">Responses received so far.</param>
    /// <exception cref="ConflictException">The session already has responses.</exception>
    /// <exception cref="ValidationException">The definition is invalid.</exception>
    public void ReplaceForm(Guid? formId, string formTitle, string? formDescription, FormDefinition definition, int responseCount)
    {
        if (responseCount > 0)
        {
            throw new ConflictException("Esta sessão já tem respostas; as perguntas não podem mais mudar. Crie uma nova sessão.");
        }

        SetForm(formId, formTitle, formDescription, definition);
    }

    /// <summary>Stops accepting responses now. Closing twice keeps the first time.</summary>
    /// <param name="utcNow">Current time in UTC.</param>
    public void Close(DateTime utcNow) => ClosedAt ??= utcNow;

    /// <summary>Undoes <see cref="Close"/>; the session follows its period and limit again.</summary>
    public void Reopen() => ClosedAt = null;

    private static string NewPublicCode() => Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(16));

    [MemberNotNull(nameof(FormTitle), nameof(Definition))]
    private void SetForm(Guid? formId, string formTitle, string? formDescription, FormDefinition definition)
    {
        definition.EnsureValid();

        FormId = formId;
        FormTitle = formTitle;
        FormDescription = formDescription;
        Definition = definition;
    }
}
