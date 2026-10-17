using Klf.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Klf.Infrastructure.Persistence.Configurations;

internal sealed class DataRequestConfiguration : IEntityTypeConfiguration<DataRequest>
{
    public void Configure(EntityTypeBuilder<DataRequest> builder)
    {
        builder.Property(x => x.Protocol).HasMaxLength(9);
        builder.Property(x => x.Name).HasMaxLength(120);
        builder.Property(x => x.Email).HasMaxLength(200);
        builder.Property(x => x.Details).HasMaxLength(2000);
        builder.Property(x => x.Resolution).HasMaxLength(2000);
        builder.Property(x => x.History).HasJsonConversion();

        builder.HasIndex(x => x.Protocol).IsUnique();
        builder.HasIndex(x => new { x.Status, x.DueOn });
    }
}
