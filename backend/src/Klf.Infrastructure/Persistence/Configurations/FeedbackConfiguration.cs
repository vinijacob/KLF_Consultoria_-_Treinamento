using Klf.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Klf.Infrastructure.Persistence.Configurations;

internal sealed class FeedbackFormConfiguration : IEntityTypeConfiguration<FeedbackForm>
{
    public void Configure(EntityTypeBuilder<FeedbackForm> builder)
    {
        builder.Property(x => x.Title).HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.Definition).HasJsonConversion();
    }
}

internal sealed class FeedbackSessionConfiguration : IEntityTypeConfiguration<FeedbackSession>
{
    public void Configure(EntityTypeBuilder<FeedbackSession> builder)
    {
        builder.Property(x => x.Title).HasMaxLength(200);
        builder.Property(x => x.PublicCode).HasMaxLength(FeedbackSession.PublicCodeLength).IsFixedLength();
        builder.Property(x => x.FormTitle).HasMaxLength(200);
        builder.Property(x => x.FormDescription).HasMaxLength(1000);
        builder.Property(x => x.Definition).HasJsonConversion();

        builder.HasIndex(x => x.PublicCode).IsUnique();
        builder.HasIndex(x => x.OpensAt);

        builder.HasOne<FeedbackForm>().WithMany().HasForeignKey(x => x.FormId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Client>().WithMany().HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Service>().WithMany().HasForeignKey(x => x.ServiceId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class FeedbackResponseConfiguration : IEntityTypeConfiguration<FeedbackResponse>
{
    public void Configure(EntityTypeBuilder<FeedbackResponse> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Answers).HasJsonConversion();

        builder.HasIndex(x => x.SessionId);

        builder.HasOne<FeedbackSession>().WithMany().HasForeignKey(x => x.SessionId).OnDelete(DeleteBehavior.Restrict);
    }
}
