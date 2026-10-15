using Klf.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Klf.Infrastructure.Persistence.Configurations;

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.Property(x => x.ActorName).HasMaxLength(150);
        builder.Property(x => x.Action).HasMaxLength(60);
        builder.Property(x => x.TargetLabel).HasMaxLength(200);

        builder.HasIndex(x => x.OccurredAt);
        builder.HasIndex(x => new { x.ActorId, x.OccurredAt });
    }
}
