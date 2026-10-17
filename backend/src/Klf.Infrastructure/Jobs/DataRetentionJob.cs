using Klf.Application.Interfaces.Audit;
using Klf.Application.Services.Audit;
using Klf.Application.Services.Auth;
using Klf.Application.Services.Contact;
using Klf.Application.Services.DataRequests;
using Klf.Domain.Common;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Klf.Infrastructure.Jobs;

/// <summary>
/// Once a day, deletes what is past its retention period (LGPD): contact messages (2 years after the last contact),
/// audit log entries (2 years), known devices of the panel unused for a year and data requests closed 5 years ago. Runs a few minutes after the app starts so a restart does not wait a whole day.
/// Turned off with <c>Jobs:Enabled=false</c> (tests).
/// </summary>
internal sealed partial class DataRetentionJob(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<DataRetentionJob> logger) : BackgroundService
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
            var services = scope.ServiceProvider;
            var messages = await services.GetRequiredService<IContactMessageService>().PurgeExpiredAsync(cancellationToken);
            var entries = await services.GetRequiredService<IAuditLogService>().PurgeExpiredAsync(cancellationToken);
            var devices = await services.GetRequiredService<ILoginDeviceService>().PurgeUnusedAsync(cancellationToken);
            var requests = await services.GetRequiredService<IDataRequestService>().PurgeExpiredAsync(cancellationToken);

            if (messages > 0)
            {
                await services.GetRequiredService<IAuditTrail>().RecordAsync(
                    new AuditEntry(AuditActions.ContactPurged, TargetLabel: messages == 1 ? "1 mensagem" : $"{messages} mensagens"),
                    cancellationToken);
            }

            if (requests > 0)
            {
                await services.GetRequiredService<IAuditTrail>().RecordAsync(
                    new AuditEntry(AuditActions.DataRequestPurged, TargetLabel: requests == 1 ? "1 pedido" : $"{requests} pedidos"),
                    cancellationToken);
            }

            if (messages > 0 || entries > 0 || devices > 0 || requests > 0)
            {
                LogDeleted(logger, messages, entries, devices, requests);
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            LogFailure(logger, exception);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Retenção: {Messages} mensagem(ns) de contato, {Entries} registro(s) de auditoria, {Devices} aparelho(s) sem uso e {Requests} pedido(s) LGPD excluídos.")]
    private static partial void LogDeleted(ILogger logger, int messages, int entries, int devices, int requests);

    [LoggerMessage(Level = LogLevel.Error, Message = "Falha na limpeza de dados vencidos; tenta de novo amanhã.")]
    private static partial void LogFailure(ILogger logger, Exception exception);
}
