using Klf.Application.Interfaces.Identity;

using Microsoft.Extensions.Options;

namespace Klf.Infrastructure.Identity;

/// <summary>Two-factor settings, bound from the <c>TwoFactor</c> configuration section.</summary>
public sealed class TwoFactorOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "TwoFactor";

    /// <summary>When <see langword="true"/> (the default), nobody gets a session without two-factor: users must enroll the authenticator app at first sign-in.</summary>
    public bool Required { get; init; } = true;
}

internal sealed class TwoFactorPolicy(IOptions<TwoFactorOptions> options) : ITwoFactorPolicy
{
    public bool IsRequired => options.Value.Required;
}
