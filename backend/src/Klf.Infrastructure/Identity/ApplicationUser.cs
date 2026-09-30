using Microsoft.AspNetCore.Identity;

namespace Klf.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public ApplicationUser()
    {
        Id = Guid.CreateVersion7();
        SecurityStamp = Guid.NewGuid().ToString();
    }

    public required string FullName { get; set; }

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
