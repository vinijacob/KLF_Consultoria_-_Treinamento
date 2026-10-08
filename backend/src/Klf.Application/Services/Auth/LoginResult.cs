namespace Klf.Application.Services.Auth;

/// <summary>Result of the password step: a session, or a two-factor step still pending.</summary>
/// <param name="Session">The session, when no second step is needed.</param>
/// <param name="Challenge">The pending second step, when <paramref name="Session"/> is empty.</param>
public sealed record LoginResult(AuthSession? Session, TwoFactorChallenge? Challenge);

/// <summary>A second step the user still has to complete before getting a session.</summary>
/// <param name="Token">Short-lived token to send with the second step.</param>
/// <param name="SetupRequired"><see langword="true"/> if the user must first enroll the authenticator app; <see langword="false"/> if it only needs the code.</param>
public sealed record TwoFactorChallenge(string Token, bool SetupRequired);

/// <summary>Two-factor was just enabled: the new session and the recovery codes (shown once).</summary>
/// <param name="Session">The session started by the enrollment.</param>
/// <param name="RecoveryCodes">The one-time recovery codes.</param>
public sealed record TwoFactorEnabledResult(AuthSession Session, IReadOnlyList<string> RecoveryCodes);
