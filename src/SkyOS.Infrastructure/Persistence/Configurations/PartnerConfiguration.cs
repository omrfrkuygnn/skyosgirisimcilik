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
        builder.Property(x => x.SupportLetterUrl).HasMaxLength(400);

        builder.HasIndex(x => x.DisplayOrder);

        builder.HasData(
            new Partner
            {
                Id = 1,
                Name = "HAVELSAN",
                LogoUrl = "/img/partners/havelsan.svg",
                Description = "HAVELSAN ile teknik değerlendirme görüşmesi gerçekleştirildi ve Jet Cube hızlandırıcı programına kabul edildi.",
                WebsiteUrl = "https://www.havelsan.com/tr",
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
                WebsiteUrl = "https://harven.com.tr/",
                SupportLetterUrl = "/docs/destek-mektubu-harven.pdf",
                IsVerifiedRelationship = true,
                DisplayOrder = 2,
                CreatedAtUtc = SeedTimestamps.Initial,
            },
            new Partner
            {
                Id = 6,
                Name = "Uludoğan Savunma",
                LogoUrl = "/img/partners/uludogan-logo.png",
                Description = "TRL 6 - TRL 8 seviyesinde uçuş kontrol kartı entegrasyonu, gerçek zamanlı siber dayanıklılık analizi, pilot test ev sahipliği ve ticari satın alma niyet mektubuna sahip savunma partnerimiz.",
                Address = "Cube Incubation 2. kat 10 nolu ofis - Sanayi Mah. Teknopark Bulvarı No:1/4C Pendik / İstanbul",
                Phone = "+90 (531) 776 5798",
                WebsiteUrl = "https://www.uludogantech.com",
                SupportLetterUrl = "/docs/destek-mektubu-uludogan.pdf",
                IsVerifiedRelationship = true,
                DisplayOrder = 3,
                CreatedAtUtc = SeedTimestamps.Initial,
            },
            new Partner
            {
                Id = 3,
                Name = "Erzincan Ticaret ve Sanayi Odası",
                LogoUrl = "/img/partners/erzincan-tso-logo.png",
                Description = "12 Mart 2026 tarihli 113 sayılı Yönetim Kurulu Kararı ile SkyOS yazılım altyapısının Ar-Ge, inovasyon ve yerli girişimcilik ekosistemine katkılarını onaylayan resmi oda desteği.",
                Address = "Atatürk Mah. Nermi Tombul Cad. No:20/21 Erzincan",
                Phone = "(446) 502 55 50",
                WebsiteUrl = "https://www.erzincantso.org.tr",
                SupportLetterUrl = "/docs/destek-mektubu-erzincan-tso.pdf",
                IsVerifiedRelationship = true,
                DisplayOrder = 4,
                CreatedAtUtc = SeedTimestamps.Initial,
            },
            new Partner
            {
                Id = 4,
                Name = "Erzincan Binali Yıldırım Üniversitesi",
                LogoUrl = "/img/partners/ebyu-logo.png",
                Description = "Mühendislik-Mimarlık Fakültesi Dekanlığı tarafından, Rust tabanlı siber güvenlik, modüler eklenti mimarisi ve akademik araştırma iş birliğini kapsayan resmi üniversite desteği.",
                Address = "Yalnızbağ Yerleşkesi Mühendislik-Mimarlık Fakültesi Dekanlığı, Erzincan",
                Phone = "(446) 224 00 89",
                WebsiteUrl = "https://ebyu.edu.tr/",
                SupportLetterUrl = "/docs/destek-mektubu-ebyu.pdf",
                IsVerifiedRelationship = true,
                DisplayOrder = 5,
                CreatedAtUtc = SeedTimestamps.Initial,
            },
            new Partner
            {
                Id = 5,
                Name = "T.C. Sanayi ve Teknoloji İl Müdürlüğü",
                LogoUrl = "/img/partners/sanayi-il-mudurlugu-logo.svg",
                Description = "Yerli otonom yazılım ekosisteminin güçlendirilmesi, Ar-Ge projelerinin hızlandırılması ve milli teknoloji hamlesi vizyonu doğrultusunda T.C. Sanayi ve Teknoloji Bakanlığı İl Müdürlüğü resmi desteği.",
                Address = "Erzincan Valiliği Binası Sanayi ve Teknoloji İl Müdürlüğü, Erzincan",
                Phone = "(446) 223 75 00",
                WebsiteUrl = "https://www.sanayi.gov.tr/anasayfa",
                SupportLetterUrl = "/docs/destek-mektubu-sanayi-il-mudurlugu.pdf",
                IsVerifiedRelationship = true,
                DisplayOrder = 6,
                CreatedAtUtc = SeedTimestamps.Initial,
            });
    }
}
