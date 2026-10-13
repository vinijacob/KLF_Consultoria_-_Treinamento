using Klf.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Klf.Infrastructure.Persistence.Configurations;

internal sealed class ContactMessageConfiguration : IEntityTypeConfiguration<ContactMessage>
{
    public void Configure(EntityTypeBuilder<ContactMessage> builder)
    {
        builder.Property(x => x.Name).HasMaxLength(120);
        builder.Property(x => x.Email).HasMaxLength(200);
        builder.Property(x => x.Phone).HasMaxLength(20);
        builder.Property(x => x.Company).HasMaxLength(150);
        builder.Property(x => x.ServiceTitle).HasMaxLength(200);
        builder.Property(x => x.Text).HasMaxLength(4000);
        builder.Property(x => x.PrivacyPolicyVersion).HasMaxLength(10);

        builder.HasIndex(x => x.ReceivedAt);
        builder.HasIndex(x => x.Status);

        builder.HasOne<Service>()
            .WithMany()
            .HasForeignKey(x => x.ServiceId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
