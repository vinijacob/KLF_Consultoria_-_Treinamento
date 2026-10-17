using Klf.Domain.Common;
using Klf.Domain.Enums;
using Klf.Domain.Exceptions;

namespace Klf.Domain.Entities;

/// <summary>A step taken while handling a <see cref="DataRequest"/>, shown as its history.</summary>
/// <param name="At">When it happened (UTC).</param>
/// <param name="Text">What was done, in Portuguese (e.g. "2 mensagens de contato apagadas").</param>
public sealed record DataRequestEvent(DateTime At, string Text);

/// <summary>
/// A request of a data subject about their personal data (LGPD, art. 18): access, correction, deletion or withdrawal of
/// consent. KLF must answer within <see cref="DeadlineDays"/> days. Deleted for real, <see cref="RetentionYears"/> years
/// after it was closed (kept until then as proof that it was answered).
/// </summary>
public sealed class DataRequest : Entity
{
    /// <summary>Days KLF has to answer (LGPD, art. 19, II).</summary>
    public const int DeadlineDays = 15;

    /// <summary>Closed requests are deleted this many years after being closed.</summary>
    public const int RetentionYears = 5;

    /// <summary>Records a request just sent through the site.</summary>
    /// <param name="protocol">Short random code given to the person to follow the request.</param>
    /// <param name="name">Name of the person.</param>
    /// <param name="email">E-mail used to answer (and to confirm identity).</param>
    /// <param name="kind">What is asked.</param>
    /// <param name="details">Free text from the person; optional.</param>
    /// <param name="utcNow">Current time in UTC.</param>
    public DataRequest(string protocol, string name, string email, DataRequestKind kind, string? details, DateTime utcNow)
    {
        Protocol = protocol;
        Name = name;
        Email = email;
        Kind = kind;
        Details = details;
        ReceivedAt = utcNow;
        DueOn = DateOnly.FromDateTime(AppTimeZone.ToLocal(utcNow)).AddDays(DeadlineDays);
        Status = DataRequestStatus.Open;
        History = [new DataRequestEvent(utcNow, "Pedido recebido pelo site.")];
    }

    private DataRequest()
    {
        Protocol = Name = Email = string.Empty;
        History = [];
    }

    /// <summary>Short code given to the person (e.g. <c>K7PQ-3MXA</c>).</summary>
    public string Protocol { get; private set; }

    /// <summary>Name of the person.</summary>
    public string Name { get; private set; }

    /// <summary>E-mail of the person.</summary>
    public string Email { get; private set; }

    /// <summary>What is asked.</summary>
    public DataRequestKind Kind { get; private set; }

    /// <summary>Free text from the person.</summary>
    public string? Details { get; private set; }

    /// <summary>When it arrived (UTC).</summary>
    public DateTime ReceivedAt { get; private set; }

    /// <summary>Last day to answer, in Manaus time.</summary>
    public DateOnly DueOn { get; private set; }

    /// <summary>Where it stands.</summary>
    public DataRequestStatus Status { get; private set; }

    /// <summary>When it was completed or declined (UTC).</summary>
    public DateTime? ClosedAt { get; private set; }

    /// <summary>What was answered, or why it was declined.</summary>
    public string? Resolution { get; private set; }

    /// <summary>Steps taken, oldest first.</summary>
    public IReadOnlyList<DataRequestEvent> History { get; private set; }

    /// <summary>Records a step (e.g. data deleted) in the history.</summary>
    public void Record(string text, DateTime utcNow) => History = [.. History, new DataRequestEvent(utcNow, text)];

    /// <summary>Completes, declines or reopens the request.</summary>
    /// <exception cref="ValidationException">Declining without saying why.</exception>
    public void ChangeStatus(DataRequestStatus status, string? resolution, DateTime utcNow)
    {
        if (status == DataRequestStatus.Declined && string.IsNullOrWhiteSpace(resolution))
        {
            throw new ValidationException("Resolution", "Explique por que o pedido não foi atendido. A pessoa precisa receber esse motivo.");
        }

        if (status == Status && resolution == Resolution)
        {
            return;
        }

        Status = status;
        Resolution = status == DataRequestStatus.Open ? null : resolution;
        ClosedAt = status == DataRequestStatus.Open ? null : utcNow;
        Record(
            status switch
            {
                DataRequestStatus.Completed => "Pedido concluído.",
                DataRequestStatus.Declined => "Pedido recusado.",
                _ => "Pedido reaberto.",
            },
            utcNow);
    }

    /// <summary>Whether the deadline passed without an answer.</summary>
    public bool IsOverdue(DateOnly today) => Status == DataRequestStatus.Open && today > DueOn;

    /// <summary>Closed requests closed before this moment are past the retention period.</summary>
    public static DateTime RetentionCutoff(DateTime utcNow) => utcNow.AddYears(-RetentionYears);
}
