using Klf.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Klf.Infrastructure.Persistence.Configurations;

internal sealed class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.Property(x => x.Name).HasMaxLength(200);
        builder.Property(x => x.WebsiteUrl).HasMaxLength(300);

        builder.HasIndex(x => new { x.IsActive, x.DisplayOrder });

        builder.HasOne<MediaAsset>()
            .WithMany()
            .HasForeignKey(x => x.LogoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
