namespace Klf.Application.DTOs.Auth;

/// <summary>
/// Result of the password step. Either a session (<see cref="AccessToken"/>), or a request for the second step:
/// <see cref="TwoFactorRequired"/> (send the authenticator code) or <see cref="TwoFactorSetupRequired"/> (enroll the app first).
/// The refresh token travels only in the HttpOnly cookie.
/// </summary>
/// <param name="AccessToken">JWT to send in the <c>Authorization: Bearer</c> header; empty while a second step is pending.</param>
/// <param name="ExpiresAt">When the access token expires, in UTC; empty while a second step is pending.</param>
/// <param name="TwoFactorRequired">The user must send the code of the authenticator app (or a recovery code) to <c>/auth/2fa/verify</c> or <c>/auth/2fa/recover</c>.</param>
/// <param name="TwoFactorSetupRequired">The user must register the authenticator app with <c>/auth/2fa/setup</c> and <c>/auth/2fa/enable</c> first.</param>
/// <param name="TwoFactorToken">Short-lived (5 min) token to send with the second step; empty when a session was issued.</param>
public sealed record LoginResponse(
    string? AccessToken,
    DateTime? ExpiresAt,
    bool TwoFactorRequired = false,
    bool TwoFactorSetupRequired = false,
    string? TwoFactorToken = null);
