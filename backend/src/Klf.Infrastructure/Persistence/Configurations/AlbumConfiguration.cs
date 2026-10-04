using Klf.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Klf.Infrastructure.Persistence.Configurations;

internal sealed class AlbumConfiguration : IEntityTypeConfiguration<Album>
{
    public void Configure(EntityTypeBuilder<Album> builder)
    {
        builder.Property(x => x.Title).HasMaxLength(200);
        builder.Property(x => x.Slug).HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(500);

        builder.HasIndex(x => x.Slug)
            .IsUnique()
            .HasFilter("\"DeletedAt\" IS NULL");

        builder.HasIndex(x => new { x.IsActive, x.DisplayOrder });

        builder.HasOne<MediaAsset>()
            .WithMany()
            .HasForeignKey(x => x.CoverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(x => x.AlbumId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class AlbumMediaConfiguration : IEntityTypeConfiguration<AlbumMedia>
{
    public void Configure(EntityTypeBuilder<AlbumMedia> builder)
    {
        builder.Property(x => x.Caption).HasMaxLength(300);

        builder.HasIndex(x => new { x.AlbumId, x.MediaAssetId }).IsUnique();

        builder.HasOne<MediaAsset>()
            .WithMany()
            .HasForeignKey(x => x.MediaAssetId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
