using Klf.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Klf.Infrastructure.Persistence.Configurations;

internal sealed class TestimonialConfiguration : IEntityTypeConfiguration<Testimonial>
{
    public void Configure(EntityTypeBuilder<Testimonial> builder)
    {
        builder.Property(x => x.AuthorName).HasMaxLength(120);
        builder.Property(x => x.AuthorRole).HasMaxLength(120);
        builder.Property(x => x.CompanyName).HasMaxLength(200);
        builder.Property(x => x.Quote).HasMaxLength(1000);

        builder.HasIndex(x => new { x.IsPublished, x.DisplayOrder });

        builder.HasOne<MediaAsset>()
            .WithMany()
            .HasForeignKey(x => x.PhotoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
