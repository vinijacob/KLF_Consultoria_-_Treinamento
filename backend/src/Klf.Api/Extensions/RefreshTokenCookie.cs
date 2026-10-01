namespace Klf.Api.Extensions;

/// <summary>
/// Reads and writes the refresh token cookie: HttpOnly (invisible to JavaScript), Secure (HTTPS only),
/// SameSite=Strict (not sent by other sites, which blocks CSRF) and limited to the auth routes.
/// </summary>
internal static class RefreshTokenCookie
{
    /// <summary>Cookie name.</summary>
    public const string Name = "klf_refresh";

    /// <summary>The browser only sends the cookie to routes under this path.</summary>
    public const string Path = "/api/v1/auth";

    /// <summary>Stores the refresh token until the end of the session.</summary>
    public static void Append(HttpResponse response, string refreshToken, DateTime sessionExpiresAt) =>
        response.Cookies.Append(Name, refreshToken, CreateOptions(sessionExpiresAt));

    /// <summary>Returns the refresh token sent by the browser, or <see langword="null"/>.</summary>
    public static string? Read(HttpRequest request) =>
        request.Cookies.TryGetValue(Name, out var value) && !string.IsNullOrWhiteSpace(value) ? value : null;

    /// <summary>Asks the browser to discard the cookie.</summary>
    public static void Delete(HttpResponse response) => response.Cookies.Delete(Name, CreateOptions(expires: null));

    private static CookieOptions CreateOptions(DateTime? expires) => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Path = Path,
        IsEssential = true,
        Expires = expires,
    };
}
