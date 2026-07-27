using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SkyOS.Application.Interfaces.Infrastructure;
using SkyOS.Domain.Common;
using SkyOS.Domain.Entities;
using SkyOS.Infrastructure.Identity;

namespace SkyOS.Infrastructure.Persistence;

public sealed class SkyOSDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public SkyOSDbContext(DbContextOptions<SkyOSDbContext> options, IDateTimeProvider dateTimeProvider)
        : base(options)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();

    public DbSet<Partner> Partners => Set<Partner>();

    public DbSet<Milestone> Milestones => Set<Milestone>();

    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();

    public DbSet<NewsItem> NewsItems => Set<NewsItem>();

    public DbSet<SiteFeedback> SiteFeedbacks => Set<SiteFeedback>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // All entity configurations are Fluent API classes (IEntityTypeConfiguration<T>);
        // no data-annotation attributes on the domain entities.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SkyOSDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyAuditTimestamps();
        return base.SaveChanges();
    }

    private void ApplyAuditTimestamps()
    {
        var now = _dateTimeProvider.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAtUtc = now;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAtUtc = now;
                    break;
            }
        }
    }
}
