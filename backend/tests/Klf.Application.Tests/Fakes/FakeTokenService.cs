using Klf.Application.Interfaces.Identity;

namespace Klf.Application.Tests.Fakes;

internal sealed class FakeTokenService(TimeProvider clock) : ITokenService
{
    private int _counter;

    public static string Hash(string token) => $"hash:{token}";

    public AccessToken Generate(UserAccount user) => new($"access-for-{user.Id}", clock.GetUtcNow().UtcDateTime.AddMinutes(15));

    public NewRefreshToken CreateRefreshToken()
    {
        var token = $"refresh-{++_counter}";
        return new NewRefreshToken(token, Hash(token), clock.GetUtcNow().UtcDateTime.AddHours(8));
    }

    public string HashRefreshToken(string token) => Hash(token);

    public string CreateTwoFactorChallenge(Guid userId) => $"challenge-{userId}";

    public Task<Guid?> ReadTwoFactorChallengeAsync(string token) =>
        Task.FromResult<Guid?>(token.StartsWith("challenge-", StringComparison.Ordinal) && Guid.TryParse(token["challenge-".Length..], out var id) ? id : null);
}

internal sealed class FakeTwoFactorPolicy(bool isRequired) : ITwoFactorPolicy
{
    public bool IsRequired => isRequired;
}
