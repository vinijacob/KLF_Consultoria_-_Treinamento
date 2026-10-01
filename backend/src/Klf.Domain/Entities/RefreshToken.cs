using Klf.Domain.Common;

namespace Klf.Domain.Entities;

/// <summary>
/// A refresh token of an admin panel session. Only the SHA-256 hash is stored; the token itself lives in an HttpOnly cookie.
/// </summary>
/// <remarks>
/// Every refresh replaces the token with a new one from the same <see cref="FamilyId"/> (one family = one sign-in).
/// Presenting a token that was already exchanged (<see cref="UsedAt"/>) means it was stolen, so the sessions must be revoked.
/// A token that was only revoked (logout) is simply rejected.
/// </remarks>
public sealed class RefreshToken : Entity
{
    /// <summary>Creates an unused refresh token.</summary>
    public RefreshToken(Guid userId, Guid familyId, string tokenHash, DateTime expiresAt)
    {
        UserId = userId;
        FamilyId = familyId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
    }

    /// <summary>Owner of the session.</summary>
    public Guid UserId { get; private set; }

    /// <summary>Groups every token issued for the same sign-in.</summary>
    public Guid FamilyId { get; private set; }

    /// <summary>SHA-256 hash of the token, in hexadecimal.</summary>
    public string TokenHash { get; private set; }

    /// <summary>End of the session, in UTC. Rotated tokens inherit it, so refreshing never extends a session.</summary>
    public DateTime ExpiresAt { get; private set; }

    /// <summary>When the token was exchanged for a new one; <see langword="null"/> while unused.</summary>
    public DateTime? UsedAt { get; private set; }

    /// <summary>When the token was revoked (logout or suspected theft); <see langword="null"/> otherwise.</summary>
    public DateTime? RevokedAt { get; private set; }

    /// <summary>Whether the token was already exchanged or revoked, so it can no longer be used.</summary>
    public bool WasConsumed => UsedAt is not null || RevokedAt is not null;

    /// <summary>Whether the token can still be exchanged at <paramref name="utcNow"/>.</summary>
    public bool IsActive(DateTime utcNow) => !WasConsumed && utcNow < ExpiresAt;

    /// <summary>Marks the token as exchanged for a new one.</summary>
    public void MarkAsUsed(DateTime utcNow) => UsedAt ??= utcNow;

    /// <summary>Revokes the token. Does nothing if it is already revoked.</summary>
    public void Revoke(DateTime utcNow) => RevokedAt ??= utcNow;
}
