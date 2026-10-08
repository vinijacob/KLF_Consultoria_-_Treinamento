namespace Klf.Application.DTOs.Auth;

/// <summary>Data to register the authenticator app.</summary>
/// <param name="SharedKey">Secret key to type by hand, in groups of four characters.</param>
/// <param name="OtpAuthUri">The <c>otpauth://</c> address to render as a QR Code. It contains the secret: never log it or send it to third parties.</param>
public sealed record TwoFactorSetupResponse(string SharedKey, string OtpAuthUri);

/// <summary>Two-factor was enabled: the session starts and the recovery codes are shown, once.</summary>
/// <param name="AccessToken">JWT to send in the <c>Authorization: Bearer</c> header.</param>
/// <param name="ExpiresAt">When the access token expires, in UTC.</param>
/// <param name="RecoveryCodes">Ten one-time codes. They are never shown again: the user must save them now.</param>
public sealed record TwoFactorEnabledResponse(string AccessToken, DateTime ExpiresAt, IReadOnlyList<string> RecoveryCodes);

/// <summary>New recovery codes.</summary>
/// <param name="RecoveryCodes">Ten one-time codes. They are never shown again; the previous ones no longer work.</param>
public sealed record RecoveryCodesResponse(IReadOnlyList<string> RecoveryCodes);
