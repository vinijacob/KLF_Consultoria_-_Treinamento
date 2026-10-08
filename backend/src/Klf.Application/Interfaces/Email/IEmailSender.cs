namespace Klf.Application.Interfaces.Email;

/// <summary>An e-mail to send.</summary>
/// <param name="To">Recipient address.</param>
/// <param name="Subject">Subject line.</param>
/// <param name="HtmlBody">Message as HTML. Any user-provided text must be HTML-encoded by the caller.</param>
/// <param name="TextBody">The same message as plain text.</param>
public sealed record EmailMessage(string To, string Subject, string HtmlBody, string TextBody);

/// <summary>Sends e-mails. Implemented in Infrastructure; sending is queued, so callers do not wait for the provider.</summary>
public interface IEmailSender
{
    /// <summary>Queues an e-mail for delivery.</summary>
    /// <param name="message">The e-mail.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken);
}
