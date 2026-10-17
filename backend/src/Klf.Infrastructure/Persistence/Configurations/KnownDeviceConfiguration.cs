using Klf.Domain.Entities;
using Klf.Infrastructure.Identity;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Klf.Infrastructure.Persistence.Configurations;

internal sealed class KnownDeviceConfiguration : IEntityTypeConfiguration<KnownDevice>
{
    public void Configure(EntityTypeBuilder<KnownDevice> builder)
    {
        builder.Property(x => x.TokenHash).HasMaxLength(64);
        builder.Property(x => x.Description).HasMaxLength(120);

        builder.HasIndex(x => new { x.UserId, x.TokenHash }).IsUnique();
        builder.HasIndex(x => x.LastSeenAt);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
