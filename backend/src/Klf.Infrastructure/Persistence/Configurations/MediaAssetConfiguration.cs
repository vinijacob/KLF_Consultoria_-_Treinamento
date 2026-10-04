using Klf.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Klf.Infrastructure.Persistence.Configurations;

internal sealed class MediaAssetConfiguration : IEntityTypeConfiguration<MediaAsset>
{
    public void Configure(EntityTypeBuilder<MediaAsset> builder)
    {
        builder.Property(x => x.StorageKey).HasMaxLength(300);
        builder.Property(x => x.OriginalFileName).HasMaxLength(200);
        builder.Property(x => x.ContentType).HasMaxLength(100);
        builder.Property(x => x.AltText).HasMaxLength(200);

        builder.HasIndex(x => x.StorageKey).IsUnique();
    }
}
