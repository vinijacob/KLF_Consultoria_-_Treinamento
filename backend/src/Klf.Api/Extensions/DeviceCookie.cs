using Klf.Domain.Entities;

namespace Klf.Api.Extensions;

/// <summary>
/// Reads and writes the cookie that identifies a browser that already signed in to the panel (new-device alert):
/// HttpOnly, Secure, SameSite=Strict and limited to the auth routes, like the refresh token. Lasts a year from the
/// last sign-in.
/// </summary>
internal static class DeviceCookie
{
    /// <summary>Cookie name.</summary>
    public const string Name = "klf_device";

    /// <summary>Stores the device token, renewing the expiry.</summary>
    public static void Append(HttpResponse response, string deviceToken) =>
        response.Cookies.Append(Name, deviceToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = RefreshTokenCookie.Path,
            IsEssential = true,
            MaxAge = TimeSpan.FromDays(KnownDevice.RetentionDays),
        });

    /// <summary>Returns the device token sent by the browser, or <see langword="null"/>.</summary>
    public static string? Read(HttpRequest request) =>
        request.Cookies.TryGetValue(Name, out var value) && !string.IsNullOrWhiteSpace(value) ? value : null;
}
