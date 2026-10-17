using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;

using Klf.Application.Interfaces.Email;
using Klf.Application.Interfaces.Identity;
using Klf.Application.Interfaces.Links;
using Klf.Application.Interfaces.Repositories;
using Klf.Domain.Common;
using Klf.Domain.Entities;

namespace Klf.Application.Services.Auth;

internal sealed class LoginDeviceService(
    IKnownDeviceRepository devices,
    IClientDevice client,
    IEmailSender emailSender,
    IFrontendLinks links,
    TimeProvider timeProvider) : ILoginDeviceService
{
    private const int TokenBytes = 32;

    public async Task<DeviceSignIn> RegisterSignInAsync(Guid userId, string email, string fullName, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var description = DeviceDescription.FromUserAgent(client.UserAgent);
        var token = IsWellFormed(client.DeviceToken) ? client.DeviceToken! : NewToken();
        var hash = Hash(token);

        if (await devices.FindAsync(userId, hash, cancellationToken) is { } known)
        {
            known.MarkSeen(description, now);

            return new DeviceSignIn(token, null);
        }

        var firstDevice = !await devices.AnyAsync(userId, cancellationToken);
        devices.Add(new KnownDevice(userId, hash, description, now));

        if (firstDevice)
        {
            return new DeviceSignIn(token, null);
        }

        await emailSender.SendAsync(
            SecurityEmailTemplates.NewDeviceSignIn(email, fullName, description, AppTimeZone.ToLocal(now), links.ForgotPassword()),
            cancellationToken);

        return new DeviceSignIn(token, description);
    }

    public Task<int> PurgeUnusedAsync(CancellationToken cancellationToken) =>
        devices.DeleteLastSeenBeforeAsync(KnownDevice.RetentionCutoff(timeProvider.GetUtcNow().UtcDateTime), cancellationToken);

    private static string NewToken() => Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(TokenBytes));

    private static bool IsWellFormed(string? token) =>
        token is { Length: 43 } && Base64Url.IsValid(token, out var length) && length == TokenBytes;

    private static string Hash(string token) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
