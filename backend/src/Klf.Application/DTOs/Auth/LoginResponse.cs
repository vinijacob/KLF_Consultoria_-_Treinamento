namespace Klf.Application.DTOs.Auth;

/// <summary>Access token issued after a sign-in or a session refresh. The refresh token travels only in the HttpOnly cookie.</summary>
/// <param name="AccessToken">JWT to send in the <c>Authorization: Bearer</c> header.</param>
/// <param name="ExpiresAt">When the token expires, in UTC.</param>
public sealed record LoginResponse(string AccessToken, DateTime ExpiresAt);
