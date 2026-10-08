using System.Net.Http.Headers;
using System.Net.Http.Json;

using Klf.Application.Interfaces.Email;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Klf.Infrastructure.Email;

/// <summary>Delivers one e-mail to the provider.</summary>
internal interface IEmailTransport
{
    Task DeliverAsync(EmailMessage message, CancellationToken cancellationToken);
}

internal sealed partial class LogEmailTransport(ILogger<LogEmailTransport> logger) : IEmailTransport
{
    public Task DeliverAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        LogEmail(logger, message.To, message.Subject, message.TextBody);

        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "E-mail (provider Log, not sent) to {To} | {Subject}\n{Body}")]
    private static partial void LogEmail(ILogger logger, string to, string subject, string body);
}

internal sealed class ResendEmailTransport(HttpClient httpClient, IOptions<EmailOptions> options) : IEmailTransport
{
    public async Task DeliverAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        var settings = options.Value;

        if (string.IsNullOrWhiteSpace(settings.From) || string.IsNullOrWhiteSpace(settings.ResendApiKey))
        {
            throw new InvalidOperationException("Email:From e Email:ResendApiKey não configurados. Use user-secrets ou variáveis de ambiente.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "emails")
        {
            Content = JsonContent.Create(new
            {
                from = settings.From,
                to = new[] { message.To },
                subject = message.Subject,
                html = message.HtmlBody,
                text = message.TextBody,
            }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ResendApiKey);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
