using System.Security.Cryptography;

using Microsoft.AspNetCore.DataProtection;

namespace Klf.Api.Extensions;

/// <summary>
/// Marks that this browser already answered a feedback session, without identifying anyone: the cookie holds only the
/// session code, encrypted and signed with Data Protection (so it cannot be forged), and is sent only to that session's routes.
/// </summary>
public sealed class FeedbackCookie(IDataProtectionProvider dataProtection)
{
    private const string Prefix = "klf_fb_";
    private const string PathPrefix = "/api/v1/public/feedback/";

    private readonly IDataProtector _protector = dataProtection.CreateProtector("Klf.FeedbackAnswered.v1");

    /// <summary>Whether the request carries a valid mark for the session.</summary>
    public bool HasAnswered(HttpRequest request, string publicCode)
    {
        if (!request.Cookies.TryGetValue(Prefix + publicCode, out var value) || string.IsNullOrEmpty(value))
        {
            return false;
        }

        try
        {
            return _protector.Unprotect(value) == publicCode;
        }
        catch (CryptographicException)
        {
            return false;
        }
    }

    /// <summary>Stores the mark until <paramref name="expires"/>.</summary>
    public void MarkAnswered(HttpResponse response, string publicCode, DateTime expires) =>
        response.Cookies.Append(Prefix + publicCode, _protector.Protect(publicCode), new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Path = PathPrefix + publicCode,
            IsEssential = true,
            Expires = expires,
        });
}
