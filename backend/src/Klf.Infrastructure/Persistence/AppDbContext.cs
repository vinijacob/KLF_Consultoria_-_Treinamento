using System.Linq.Expressions;

using Klf.Application.Interfaces.Repositories;
using Klf.Domain.Common;
using Klf.Domain.Entities;
using Klf.Infrastructure.Identity;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Klf.Infrastructure.Persistence;

/// <summary>
/// EF Core database context. Includes the Identity tables; entity mappings live in
/// <c>Persistence/Configurations</c> as <see cref="IEntityTypeConfiguration{TEntity}"/> classes and are picked up automatically.
/// </summary>
/// <remarks>
/// Applies three project-wide rules so services don't have to:
/// enums are stored as text, soft-deleted rows are hidden from every query,
/// and <see cref="SaveChangesAsync(CancellationToken)"/> stamps <see cref="Entity.UpdatedAt"/> and turns deletes of
/// <see cref="SoftDeletableEntity"/> into soft deletes.
/// </remarks>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options), IUnitOfWork
{
    /// <summary>Kilciene's career timeline.</summary>
    public DbSet<CareerEntry> CareerEntries => Set<CareerEntry>();

    /// <summary>Blog posts, projects and news.</summary>
    public DbSet<Post> Posts => Set<Post>();

    /// <summary>Training and consulting services offered by KLF.</summary>
    public DbSet<Service> Services => Set<Service>();

    /// <summary>Site settings (institutional texts, contact, social networks, default SEO), one row per key.</summary>
    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();

    /// <summary>Companies and stores trained by KLF.</summary>
    public DbSet<Client> Clients => Set<Client>();

    /// <summary>Named testimonials, with the consent of their authors.</summary>
    public DbSet<Testimonial> Testimonials => Set<Testimonial>();

    /// <summary>Refresh tokens of the admin panel sessions (hashes only).</summary>
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<Enum>().HaveConversion<string>().HaveMaxLength(50);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        foreach (var entityType in builder.Model.GetEntityTypes()
            .Where(t => typeof(SoftDeletableEntity).IsAssignableFrom(t.ClrType) && t.BaseType is null))
        {
            var entity = Expression.Parameter(entityType.ClrType, "entity");
            var isActive = Expression.Equal(
                Expression.Property(entity, nameof(SoftDeletableEntity.DeletedAt)),
                Expression.Constant(null, typeof(DateTime?)));

            builder.Entity(entityType.ClrType).HasQueryFilter(Expression.Lambda(isActive, entity));
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditRules();

        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyAuditRules();

        return base.SaveChanges();
    }

    private void ApplyAuditRules()
    {
        foreach (var entry in ChangeTracker.Entries<Entity>())
        {
            if (entry.State == EntityState.Deleted && entry.Entity is SoftDeletableEntity softDeletable)
            {
                entry.State = EntityState.Modified;
                softDeletable.SoftDelete();
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Property(e => e.UpdatedAt).CurrentValue = DateTime.UtcNow;
            }
        }
    }
}
