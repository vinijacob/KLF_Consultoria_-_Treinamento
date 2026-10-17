using Klf.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Klf.Infrastructure.Persistence.Configurations;

internal sealed class FeaturedQuoteConfiguration : IEntityTypeConfiguration<FeaturedQuote>
{
    public void Configure(EntityTypeBuilder<FeaturedQuote> builder)
    {
        builder.Property(x => x.Text).HasMaxLength(400);
        builder.Property(x => x.Author).HasMaxLength(120);
        builder.Property(x => x.Source).HasMaxLength(150);
    }
}
