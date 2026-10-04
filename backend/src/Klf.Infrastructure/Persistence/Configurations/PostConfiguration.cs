using Klf.Domain.Entities;
using Klf.Infrastructure.Identity;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Klf.Infrastructure.Persistence.Configurations;

internal sealed class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.Property(x => x.Title).HasMaxLength(200);
        builder.Property(x => x.Slug).HasMaxLength(200);
        builder.Property(x => x.Summary).HasMaxLength(500);
        builder.Property(x => x.SeoTitle).HasMaxLength(60);
        builder.Property(x => x.SeoDescription).HasMaxLength(160);
        builder.Property(x => x.ContentJson).HasColumnType("jsonb");

        builder.HasIndex(x => x.Slug)
            .IsUnique()
            .HasFilter("\"DeletedAt\" IS NULL");

        builder.HasIndex(x => new { x.Status, x.PublishedAt });

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<MediaAsset>()
            .WithMany()
            .HasForeignKey(x => x.CoverId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
