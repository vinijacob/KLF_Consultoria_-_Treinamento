namespace Klf.Application.Interfaces.Identity;

/// <summary>The browser of the current request, used to recognize devices that already signed in.</summary>
public interface IClientDevice
{
    /// <summary>Device token from the browser cookie, or <see langword="null"/> when the browser has none.</summary>
    string? DeviceToken { get; }

    /// <summary>The <c>User-Agent</c> header, or <see langword="null"/>.</summary>
    string? UserAgent { get; }
}
