namespace Klf.Application.Services.Auth;

/// <summary>
/// Result of a sign-in or refresh. The controller sends <see cref="AccessToken"/> in the response body and
/// <see cref="RefreshToken"/> in an HttpOnly cookie, so the refresh token is never readable by JavaScript.
/// </summary>
/// <param name="AccessToken">Short-lived JWT.</param>
/// <param name="AccessTokenExpiresAt">When the access token expires, in UTC.</param>
/// <param name="RefreshToken">Token used to get a new access token.</param>
/// <param name="SessionExpiresAt">When the session ends and a new sign-in is required, in UTC.</param>
public sealed record AuthSession(string AccessToken, DateTime AccessTokenExpiresAt, string RefreshToken, DateTime SessionExpiresAt);
