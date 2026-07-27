using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkyOS.Domain.Entities;

namespace SkyOS.Infrastructure.Persistence.Configurations;

public sealed class SiteFeedbackConfiguration : IEntityTypeConfiguration<SiteFeedback>
{
    public void Configure(EntityTypeBuilder<SiteFeedback> builder)
    {
        builder.ToTable("SiteFeedbacks");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FullName).IsRequired().HasMaxLength(120);
        builder.Property(x => x.Email).HasMaxLength(254);
        builder.Property(x => x.Message).IsRequired().HasMaxLength(4000);
        builder.Property(x => x.IpAddress).HasMaxLength(64);
        builder.Property(x => x.Culture).IsRequired().HasMaxLength(5);

        builder.Property(x => x.Category)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.HasIndex(x => x.IsRead);
        builder.HasIndex(x => x.CreatedAtUtc);
    }
}
