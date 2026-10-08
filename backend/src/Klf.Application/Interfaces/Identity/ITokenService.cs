namespace Klf.Application.Interfaces.Identity;

/// <summary>A signed access token and its expiration.</summary>
/// <param name="Token">Encoded JWT.</param>
/// <param name="ExpiresAt">When the token expires, in UTC.</param>
public sealed record AccessToken(string Token, DateTime ExpiresAt);

/// <summary>A freshly generated refresh token.</summary>
/// <param name="Token">The random token, sent to the browser in a cookie and never stored.</param>
/// <param name="Hash">SHA-256 hash of <paramref name="Token"/>, the only form stored in the database.</param>
/// <param name="ExpiresAt">End of a new session, in UTC.</param>
public sealed record NewRefreshToken(string Token, string Hash, DateTime ExpiresAt);

/// <summary>Issues access and refresh tokens. Implemented in Infrastructure.</summary>
public interface ITokenService
{
    /// <summary>Creates a short-lived access token carrying the user's id, e-mail, name and roles.</summary>
    AccessToken Generate(UserAccount user);

    /// <summary>Creates a cryptographically random refresh token for a new session.</summary>
    NewRefreshToken CreateRefreshToken();

    /// <summary>Hashes a refresh token received from the browser, to look it up in the database.</summary>
    string HashRefreshToken(string token);

    /// <summary>
    /// Creates a 5-minute token proving the password step of the sign-in was passed. It is not an access token:
    /// it only works on the two-factor endpoints.
    /// </summary>
    string CreateTwoFactorChallenge(Guid userId);

    /// <summary>Returns the user a two-factor challenge token was issued for, or <see langword="null"/> if it is invalid or expired.</summary>
    Task<Guid?> ReadTwoFactorChallengeAsync(string token);
}
