namespace Klf.Infrastructure.Identity;

/// <summary>Short claim names used in the JWT, matching the JwtBearer validation settings.</summary>
public static class ClaimTypeNames
{
    /// <summary>Role claim; one entry per role.</summary>
    public const string Role = "role";

    /// <summary>Display name claim.</summary>
    public const string Name = "name";

    /// <summary>User identifier claim.</summary>
    public const string Subject = "sub";
}
