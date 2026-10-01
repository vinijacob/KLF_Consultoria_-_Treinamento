using System.ComponentModel.DataAnnotations;
using System.Text;

using Microsoft.IdentityModel.Tokens;

namespace Klf.Infrastructure.Identity;

/// <summary>Access and refresh token settings, bound from the <c>Jwt</c> configuration section and validated at startup.</summary>
public sealed class JwtOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Jwt";

    /// <summary>Token issuer (<c>iss</c>).</summary>
    [Required]
    public string Issuer { get; init; } = string.Empty;

    /// <summary>Token audience (<c>aud</c>).</summary>
    [Required]
    public string Audience { get; init; } = string.Empty;

    /// <summary>HMAC-SHA256 signing secret, at least 32 characters. Never commit it: use user-secrets or environment variables.</summary>
    [Required]
    [MinLength(32)]
    public string SigningKey { get; init; } = string.Empty;

    /// <summary>Access token lifetime, in minutes.</summary>
    [Range(5, 1440)]
    public int AccessTokenMinutes { get; init; } = 15;

    /// <summary>Session length, in hours: after it, the refresh token stops working and a new sign-in is required.</summary>
    [Range(1, 72)]
    public int RefreshTokenHours { get; init; } = 8;

    /// <summary>Builds the symmetric key used to sign and validate tokens.</summary>
    public SymmetricSecurityKey CreateSigningKey() => new(Encoding.UTF8.GetBytes(SigningKey));
}
