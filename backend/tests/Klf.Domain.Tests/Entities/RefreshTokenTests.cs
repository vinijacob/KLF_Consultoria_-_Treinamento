using Klf.Domain.Entities;

namespace Klf.Domain.Tests.Entities;

public sealed class RefreshTokenTests
{
    private static readonly DateTime Now = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Token_is_active_when_unused_and_not_expired()
    {
        Assert.True(Create().IsActive(Now));
    }

    [Fact]
    public void Token_is_inactive_when_session_expired()
    {
        Assert.False(Create().IsActive(Now.AddHours(9)));
    }

    [Fact]
    public void Token_is_consumed_and_inactive_when_used()
    {
        var token = Create();

        token.MarkAsUsed(Now);

        Assert.True(token.WasConsumed);
        Assert.False(token.IsActive(Now));
    }

    [Fact]
    public void First_revocation_date_is_kept_when_revoked_twice()
    {
        var token = Create();

        token.Revoke(Now);
        token.Revoke(Now.AddHours(1));

        Assert.Equal(Now, token.RevokedAt);
    }

    private static RefreshToken Create() => new(Guid.CreateVersion7(), Guid.CreateVersion7(), "hash", Now.AddHours(8));
}
