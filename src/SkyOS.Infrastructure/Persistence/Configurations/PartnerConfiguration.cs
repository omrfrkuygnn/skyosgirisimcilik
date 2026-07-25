using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkyOS.Domain.Entities;

namespace SkyOS.Infrastructure.Persistence.Configurations;

public sealed class PartnerConfiguration : IEntityTypeConfiguration<Partner>
{
    public void Configure(EntityTypeBuilder<Partner> builder)
    {
        builder.ToTable("Partners");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(160);
        builder.Property(x => x.LogoUrl).HasMaxLength(400);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.Address).HasMaxLength(500);
        builder.Property(x => x.Phone).HasMaxLength(50);
        builder.Property(x => x.WebsiteUrl).HasMaxLength(400);

        builder.HasIndex(x => x.DisplayOrder);

        builder.HasData(
            new Partner
            {
                Id = 1,
                Name = "HAVELSAN",
                LogoUrl = "/img/partners/havelsan.svg",
                Description = "HAVELSAN ile teknik değerlendirme görüşmesi gerçekleştirildi ve Jet Cube hızlandırıcı programına kabul edildi.",
                IsVerifiedRelationship = true,
                DisplayOrder = 1,
                CreatedAtUtc = SeedTimestamps.Initial,
            },
            new Partner
            {
                Id = 2,
                Name = "Harven Mühendislik",
                LogoUrl = "/img/partners/harven-logo.png",
                Description = "SkyOS otonom sistemler girişim projemizin teknik altyapı, mühendislik danışmanlığı ve saha uygulamalarında projeyi destekleyen çözüm ortağımız.",
                Address = "Çankaya Mahallesi Cinnah Caddesi Erim İş Hanı No:37/22, 06690 Çankaya/Ankara",
                Phone = "(0312) 438 22 23",
                IsVerifiedRelationship = true,
                DisplayOrder = 2,
                CreatedAtUtc = SeedTimestamps.Initial,
            });
    }
}
