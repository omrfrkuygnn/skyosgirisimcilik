using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkyOS.Domain.Entities;

namespace SkyOS.Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired().HasMaxLength(450);
        builder.Property(x => x.UserEmail).IsRequired().HasMaxLength(254);
        builder.Property(x => x.Action).IsRequired().HasMaxLength(80);
        builder.Property(x => x.EntityType).HasMaxLength(80);
        builder.Property(x => x.EntityId).HasMaxLength(80);
        builder.Property(x => x.Details).HasMaxLength(2000);
        builder.Property(x => x.IpAddress).HasMaxLength(64);

        builder.HasIndex(x => x.CreatedAtUtc);
        builder.HasIndex(x => x.UserId);
    }
}
