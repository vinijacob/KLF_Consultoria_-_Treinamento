namespace Klf.Infrastructure.Identity;

/// <summary>
/// Initial admin account, bound from <c>Seed:Admin</c>. When <see cref="Email"/> and <see cref="Password"/> are set,
/// the account is created on startup if it does not exist yet.
/// </summary>
public sealed class AdminSeedOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Seed:Admin";

    /// <summary>Admin e-mail, also used as the user name.</summary>
    public string? Email { get; init; }

    /// <summary>Initial password. Must satisfy the Identity password policy.</summary>
    public string? Password { get; init; }

    /// <summary>Name shown in the admin panel.</summary>
    public string FullName { get; init; } = "Administrador";
}
