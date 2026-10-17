using Klf.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Klf.Infrastructure.Persistence.Configurations;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.Property(x => x.Name).HasMaxLength(60);
        builder.Property(x => x.Slug).HasMaxLength(80);

        builder.HasIndex(x => x.Slug).IsUnique();
    }
}

internal sealed class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.Property(x => x.Name).HasMaxLength(40);
        builder.Property(x => x.Slug).HasMaxLength(80);

        builder.HasIndex(x => x.Slug).IsUnique();
    }
}

internal sealed class PostTagConfiguration : IEntityTypeConfiguration<PostTag>
{
    public void Configure(EntityTypeBuilder<PostTag> builder)
    {
        builder.HasIndex(x => new { x.PostId, x.TagId }).IsUnique();
        builder.HasIndex(x => x.TagId);

        builder.HasOne(x => x.Tag)
            .WithMany()
            .HasForeignKey(x => x.TagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
