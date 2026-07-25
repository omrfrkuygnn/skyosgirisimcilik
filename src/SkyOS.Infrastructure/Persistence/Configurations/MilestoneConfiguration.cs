using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkyOS.Domain.Entities;
using SkyOS.Domain.Enums;

namespace SkyOS.Infrastructure.Persistence.Configurations;

public sealed class MilestoneConfiguration : IEntityTypeConfiguration<Milestone>
{
    public void Configure(EntityTypeBuilder<Milestone> builder)
    {
        builder.ToTable("Milestones");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Description).IsRequired().HasMaxLength(2000);

        // Enum stored as string for human-readable, migration-stable data.
        builder.Property(x => x.Category)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.HasIndex(x => x.Category);
        builder.HasIndex(x => x.DisplayOrder);

        builder.HasData(
            new Milestone
            {
                Id = 1,
                Title = "TEKNOFEST — Sürü İHA",
                Description = "Sürü İHA kategorisinde gerçek uçuş testleri gerçekleştirildi.",
                Category = MilestoneCategory.Teknofest,
                DisplayOrder = 1,
                CreatedAtUtc = SeedTimestamps.Initial,
            },
            new Milestone
            {
                Id = 2,
                Title = "TEKNOFEST — Savaşan İHA",
                Description = "Savaşan İHA kategorisinde otonom hedef kilitleme algoritmaları sahada test edildi.",
                Category = MilestoneCategory.Teknofest,
                DisplayOrder = 2,
                CreatedAtUtc = SeedTimestamps.Initial,
            },
            new Milestone
            {
                Id = 3,
                Title = "Çoklu Platform Entegrasyonu",
                Description = "Hava, kara (simülasyon) ve deniz (simülasyon) olmak üzere üç farklı araç tipiyle entegrasyon doğrulandı.",
                Category = MilestoneCategory.Teknik,
                DisplayOrder = 3,
                CreatedAtUtc = SeedTimestamps.Initial,
            },
            new Milestone
            {
                Id = 4,
                Title = "Karşılaştırmalı Performans Ölçümü",
                Description = "ArduPilot ve DJI ile karşılaştırmalı performans ölçümleri yapıldı.",
                Category = MilestoneCategory.Teknik,
                DisplayOrder = 4,
                CreatedAtUtc = SeedTimestamps.Initial,
            },
            new Milestone
            {
                Id = 5,
                Title = "HAVELSAN Jet Cube Programı",
                Description = "HAVELSAN ile teknik değerlendirme görüşmesi yapıldı ve Jet Cube hızlandırıcı programına kabul edildi.",
                Category = MilestoneCategory.Kurumsal,
                DisplayOrder = 5,
                CreatedAtUtc = SeedTimestamps.Initial,
            });
    }
}
