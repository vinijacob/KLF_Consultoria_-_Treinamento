using Microsoft.AspNetCore.Identity;

namespace Klf.Infrastructure.Identity;

/// <summary>
/// Admin panel user, managed by ASP.NET Core Identity (password hash, lockout, 2FA).
/// Lives in Infrastructure because it depends on Identity; the Domain never references it.
/// </summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
    /// <summary>Creates a user with a UUID v7 identifier and a fresh security stamp.</summary>
    public ApplicationUser()
    {
        Id = Guid.CreateVersion7();
        SecurityStamp = Guid.NewGuid().ToString();
    }

    /// <summary>Name shown in the admin panel.</summary>
    public required string FullName { get; set; }

    /// <summary>When the account was created, in UTC.</summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
