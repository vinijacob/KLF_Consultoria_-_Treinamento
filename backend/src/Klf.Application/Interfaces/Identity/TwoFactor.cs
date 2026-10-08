namespace Klf.Application.Interfaces.Identity;

/// <summary>Outcome of checking a two-factor code.</summary>
public enum TwoFactorCheckStatus
{
    /// <summary>The code is valid.</summary>
    Success,

    /// <summary>The code is wrong (or the user does not exist).</summary>
    Invalid,

    /// <summary>The account is temporarily locked after too many failed attempts.</summary>
    LockedOut,
}

/// <summary>Result of finishing the enrollment or regenerating recovery codes.</summary>
/// <param name="Status">Outcome of the code check.</param>
/// <param name="RecoveryCodes">The new recovery codes; empty unless <paramref name="Status"/> is <see cref="TwoFactorCheckStatus.Success"/>.</param>
public sealed record TwoFactorEnableResult(TwoFactorCheckStatus Status, IReadOnlyList<string> RecoveryCodes);

/// <summary>Data to register the authenticator app.</summary>
/// <param name="SharedKey">Secret key to type by hand, in groups of four characters.</param>
/// <param name="OtpAuthUri">The <c>otpauth://</c> address that the frontend turns into a QR Code.</param>
public sealed record TwoFactorSetup(string SharedKey, string OtpAuthUri);

/// <summary>Whether sign-in must go through two-factor authentication. Implemented in Infrastructure from configuration.</summary>
public interface ITwoFactorPolicy
{
    /// <summary>When <see langword="true"/>, users without an authenticator enrolled must enroll before getting a session.</summary>
    bool IsRequired { get; }
}
