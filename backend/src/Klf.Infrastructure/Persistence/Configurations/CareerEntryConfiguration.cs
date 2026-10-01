using Klf.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Klf.Infrastructure.Persistence.Configurations;

internal sealed class CareerEntryConfiguration : IEntityTypeConfiguration<CareerEntry>
{
    public void Configure(EntityTypeBuilder<CareerEntry> builder)
    {
        builder.Property(x => x.Title)
            .HasMaxLength(150);

        builder.Property(x => x.Institution)
            .HasMaxLength(150);

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.HasIndex(x => new { x.EntryType, x.DisplayOrder });
    }
}
