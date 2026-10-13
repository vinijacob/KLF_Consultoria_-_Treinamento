using Klf.Application.Services.Contact;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Klf.Infrastructure.Jobs;

/// <summary>
/// Once a day, deletes the contact messages past the retention period (LGPD). Runs a few minutes after the app starts
/// so a restart does not wait a whole day. Turned off with <c>Jobs:Enabled=false</c> (tests).
/// </summary>
internal sealed partial class ContactRetentionJob(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<ContactRetentionJob> logger) : BackgroundService
{
    private static readonly TimeSpan FirstRunDelay = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan Interval = TimeSpan.FromDays(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!configuration.GetValue("Jobs:Enabled", true))
        {
            return;
        }

        await Task.Delay(FirstRunDelay, stoppingToken);
        using var timer = new PeriodicTimer(Interval);

        do
        {
            await RunOnceAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    internal async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var deleted = await scope.ServiceProvider.GetRequiredService<IContactMessageService>().PurgeExpiredAsync(cancellationToken);

            if (deleted > 0)
            {
                LogDeleted(logger, deleted);
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            LogFailure(logger, exception);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Retenção: {Count} mensagem(ns) de contato com mais de 2 anos excluída(s).")]
    private static partial void LogDeleted(ILogger logger, int count);

    [LoggerMessage(Level = LogLevel.Error, Message = "Falha ao excluir mensagens de contato vencidas; tenta de novo amanhã.")]
    private static partial void LogFailure(ILogger logger, Exception exception);
}
