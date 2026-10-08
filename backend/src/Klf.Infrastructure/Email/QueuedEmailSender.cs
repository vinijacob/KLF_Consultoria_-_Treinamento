using System.Threading.Channels;

using Klf.Application.Interfaces.Email;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Klf.Infrastructure.Email;

/// <summary>
/// In-memory queue between the request and the e-mail provider: the request only enqueues, so it answers in the same time
/// whether or not an e-mail is sent (no hint about which accounts exist) and a slow provider does not slow the API.
/// </summary>
internal sealed class EmailQueue
{
    private readonly Channel<EmailMessage> _channel = Channel.CreateBounded<EmailMessage>(new BoundedChannelOptions(200)
    {
        FullMode = BoundedChannelFullMode.Wait,
        SingleReader = true,
    });

    public ChannelReader<EmailMessage> Reader => _channel.Reader;

    public ValueTask EnqueueAsync(EmailMessage message, CancellationToken cancellationToken) =>
        _channel.Writer.WriteAsync(message, cancellationToken);
}

internal sealed class QueuedEmailSender(EmailQueue queue) : IEmailSender
{
    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken) =>
        await queue.EnqueueAsync(message, cancellationToken);
}

internal sealed partial class EmailDispatcher(
    EmailQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<EmailDispatcher> logger) : BackgroundService
{
    private const int MaxAttempts = 3;

    internal TimeSpan RetryDelay { get; init; } = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var message in queue.Reader.ReadAllAsync(stoppingToken))
        {
            await DeliverWithRetryAsync(message, stoppingToken);
        }
    }

    internal async Task DeliverWithRetryAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                await scope.ServiceProvider.GetRequiredService<IEmailTransport>().DeliverAsync(message, cancellationToken);

                return;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                LogFailure(logger, exception, attempt, MaxAttempts, message.Subject);

                if (attempt < MaxAttempts)
                {
                    await Task.Delay(RetryDelay, cancellationToken);
                }
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Falha ao enviar e-mail '{Subject}' (tentativa {Attempt} de {Max}).")]
    private static partial void LogFailure(ILogger logger, Exception exception, int attempt, int max, string subject);
}
