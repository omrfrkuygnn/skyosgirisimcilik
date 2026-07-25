using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SkyOS.Shared.Constants;

namespace SkyOS.Infrastructure.Persistence;

/// <summary>
/// Applies the schema on startup. The migrations are authored for SQL Server, so we only run
/// <c>Migrate()</c> on that provider. For the SQLite dev fallback we use <c>EnsureCreated()</c>,
/// which generates a correct SQLite schema from the model (seed data is applied either way).
/// </summary>
public static class DatabaseInitializer
{
    private const string SqlServerProviderName = "Microsoft.EntityFrameworkCore.SqlServer";

    public static async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseInitializer");
        var context = provider.GetRequiredService<SkyOSDbContext>();

        try
        {
            if (string.Equals(context.Database.ProviderName, SqlServerProviderName, StringComparison.Ordinal))
            {
                await context.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
                logger.LogInformation("SQL Server migrations applied successfully.");
            }
            else
            {
                await context.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);
                logger.LogInformation("Database schema ensured via EnsureCreated for provider {Provider}.", context.Database.ProviderName);

                // Auto-patch SQLite schema silently if columns were added after initial SQLite DB file creation
                var connection = context.Database.GetDbConnection();
                var wasOpen = connection.State == System.Data.ConnectionState.Open;
                if (!wasOpen)
                {
                    await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                }

                try
                {
                    using var command = connection.CreateCommand();
                    command.CommandText = "PRAGMA table_info(Partners);";
                    using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                    var existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                    {
                        existingColumns.Add(reader.GetString(1));
                    }
                    reader.Close();

                    if (!existingColumns.Contains("Address"))
                    {
                        await context.Database.ExecuteSqlRawAsync("ALTER TABLE Partners ADD COLUMN Address TEXT;", cancellationToken).ConfigureAwait(false);
                    }
                    if (!existingColumns.Contains("Phone"))
                    {
                        await context.Database.ExecuteSqlRawAsync("ALTER TABLE Partners ADD COLUMN Phone TEXT;", cancellationToken).ConfigureAwait(false);
                    }
                    if (!existingColumns.Contains("SupportLetterUrl"))
                    {
                        await context.Database.ExecuteSqlRawAsync("ALTER TABLE Partners ADD COLUMN SupportLetterUrl TEXT;", cancellationToken).ConfigureAwait(false);
                    }
                }
                finally
                {
                    if (!wasOpen)
                    {
                        await connection.CloseAsync().ConfigureAwait(false);
                    }
                }

                // Ensure all 6 official supporters are seeded/updated in existing SQLite database
                var seedPartners = new List<Domain.Entities.Partner>
                {
                    new Domain.Entities.Partner
                    {
                        Id = 1,
                        Name = "HAVELSAN",
                        LogoUrl = "/img/partners/havelsan.svg",
                        Description = "HAVELSAN ile teknik değerlendirme görüşmesi gerçekleştirildi ve Jet Cube hızlandırıcı programına kabul edildi.",
                        WebsiteUrl = "https://www.havelsan.com/tr",
                        IsVerifiedRelationship = true,
                        DisplayOrder = 1,
                        CreatedAtUtc = DateTime.UtcNow,
                    },
                    new Domain.Entities.Partner
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
                        CreatedAtUtc = DateTime.UtcNow,
                    },
                    new Domain.Entities.Partner
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
                        CreatedAtUtc = DateTime.UtcNow,
                    },
                    new Domain.Entities.Partner
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
                        CreatedAtUtc = DateTime.UtcNow,
                    },
                    new Domain.Entities.Partner
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
                        CreatedAtUtc = DateTime.UtcNow,
                    },
                    new Domain.Entities.Partner
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
                        CreatedAtUtc = DateTime.UtcNow,
                    }
                };

                foreach (var sp in seedPartners)
                {
                    var existing = await context.Partners.FirstOrDefaultAsync(p => p.Id == sp.Id || p.Name == sp.Name, cancellationToken).ConfigureAwait(false);
                    if (existing is null)
                    {
                        context.Partners.Add(sp);
                    }
                    else
                    {
                        existing.LogoUrl = sp.LogoUrl;
                        existing.Description = sp.Description;
                        existing.Address = sp.Address;
                        existing.Phone = sp.Phone;
                        existing.WebsiteUrl = sp.WebsiteUrl;
                        existing.SupportLetterUrl = sp.SupportLetterUrl;
                        existing.IsVerifiedRelationship = sp.IsVerifiedRelationship;
                        existing.DisplayOrder = sp.DisplayOrder;
                    }
                }

                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database initialization failed.");
            throw;
        }
    }
}
