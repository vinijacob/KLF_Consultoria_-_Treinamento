namespace Klf.Application.DTOs.Auth;

/// <summary>Second step of the sign-in with the code of the authenticator app, or the final step of the enrollment.</summary>
/// <param name="TwoFactorToken">The <c>twoFactorToken</c> received from the login.</param>
/// <param name="Code">The 6-digit code shown by the authenticator app.</param>
public sealed record TwoFactorCodeRequest(string TwoFactorToken, string Code);

/// <summary>Second step of the sign-in with a one-time recovery code (when the user lost the authenticator app).</summary>
/// <param name="TwoFactorToken">The <c>twoFactorToken</c> received from the login.</param>
/// <param name="RecoveryCode">One of the recovery codes given when two-factor was enabled; each works once.</param>
public sealed record TwoFactorRecoveryRequest(string TwoFactorToken, string RecoveryCode);

/// <summary>Request to start registering the authenticator app.</summary>
/// <param name="TwoFactorToken">The <c>twoFactorToken</c> received from the login.</param>
public sealed record TwoFactorSetupRequest(string TwoFactorToken);

/// <summary>Request to replace the recovery codes of a signed-in user.</summary>
/// <param name="Code">Current 6-digit code of the authenticator app.</param>
public sealed record RegenerateRecoveryCodesRequest(string Code);
