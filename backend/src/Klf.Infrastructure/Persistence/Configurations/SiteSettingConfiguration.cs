using Klf.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Klf.Infrastructure.Persistence.Configurations;

internal sealed class SiteSettingConfiguration : IEntityTypeConfiguration<SiteSetting>
{
    public void Configure(EntityTypeBuilder<SiteSetting> builder)
    {
        builder.Property(x => x.Key).HasMaxLength(100);
        builder.Property(x => x.ValueJson).HasColumnType("jsonb");

        builder.HasIndex(x => x.Key).IsUnique();
    }
}
