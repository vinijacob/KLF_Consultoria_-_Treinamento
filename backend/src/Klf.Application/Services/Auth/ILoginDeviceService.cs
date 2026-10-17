namespace Klf.Application.Services.Auth;

/// <summary>
/// Recognizes the browser of each sign-in and alerts the account by e-mail when it is a new one (section 9.1 of the
/// specification). The first device of an account is registered without an alert.
/// </summary>
public interface ILoginDeviceService
{
    /// <summary>
    /// Registers a successful sign-in of <paramref name="userId"/>. Does not save: the caller saves with the session.
    /// </summary>
    /// <returns>The device token for the browser cookie, and the device description when it was new and an alert was sent.</returns>
    Task<DeviceSignIn> RegisterSignInAsync(Guid userId, string email, string fullName, CancellationToken cancellationToken);

    /// <summary>Deletes the devices unused for a year. Returns how many were deleted.</summary>
    Task<int> PurgeUnusedAsync(CancellationToken cancellationToken);
}

/// <summary>Result of <see cref="ILoginDeviceService.RegisterSignInAsync"/>.</summary>
/// <param name="Token">Device token for the browser cookie (renewed on every sign-in).</param>
/// <param name="NewDevice">Description of the device when the account did not know it and an alert was sent; otherwise <see langword="null"/>.</param>
public sealed record DeviceSignIn(string Token, string? NewDevice);
