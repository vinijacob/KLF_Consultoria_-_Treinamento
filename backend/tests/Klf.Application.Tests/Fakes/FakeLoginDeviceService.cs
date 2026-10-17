using Klf.Application.Services.Auth;

namespace Klf.Application.Tests.Fakes;

internal sealed class FakeLoginDeviceService : ILoginDeviceService
{
    public string? NewDevice { get; set; }

    public Task<DeviceSignIn> RegisterSignInAsync(Guid userId, string email, string fullName, CancellationToken cancellationToken) =>
        Task.FromResult(new DeviceSignIn("device-token", NewDevice));

    public Task<int> PurgeUnusedAsync(CancellationToken cancellationToken) => Task.FromResult(0);
}
