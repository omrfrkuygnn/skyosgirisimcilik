using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkyOS.Domain.Entities;

namespace SkyOS.Infrastructure.Persistence.Configurations;

public sealed class ContactMessageConfiguration : IEntityTypeConfiguration<ContactMessage>
{
    public void Configure(EntityTypeBuilder<ContactMessage> builder)
    {
        builder.ToTable("ContactMessages");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FullName).IsRequired().HasMaxLength(120);
        builder.Property(x => x.Company).HasMaxLength(160);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(254);
        builder.Property(x => x.Phone).HasMaxLength(30);
        builder.Property(x => x.Message).IsRequired().HasMaxLength(4000);
        builder.Property(x => x.IpAddress).HasMaxLength(64);

        builder.Property(x => x.InterestType)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        // Supports the per-IP spam-window count query efficiently.
        builder.HasIndex(x => new { x.IpAddress, x.CreatedAtUtc });
        builder.HasIndex(x => x.IsRead);
    }
}
