# SkyOS — Kurumsal / Yatırımcı Web Sitesi

> **SkyOS — The System for Autonomous Machines**
> Hava, kara ve deniz araçlarını tek bir yazılım omurgasından yöneten, modüler ve donanım bağımsız yerli otonom sistem platformunun kurumsal web sitesi.

ASP.NET Core 9.0 (MVC + Razor) üzerine, temiz **N-Tier / Katmanlı Mimari + Repository & Unit of Work** desenleriyle inşa edilmiştir. Güvenlik, kod kalitesi ve performans birinci önceliktir.

---

## 1. Teknoloji Yığını

| Alan | Teknoloji |
| --- | --- |
| Backend | ASP.NET Core 9.0 (MVC + Razor) |
| ORM | Entity Framework Core 9.0 (Code-First, Migrations) |
| Veritabanı | SQL Server (Prod) / SQLite (Dev) |
| Mimari | N-Tier + Repository + Unit of Work + Dependency Inversion |
| Mapping | Mapster |
| Validasyon | FluentValidation (server-side) + jQuery Unobtrusive (client-side) |
| Loglama | Serilog (Console + Rolling File) |
| Frontend | Razor SSR + özel CSS tasarım sistemi + minimal vanilla JS |
| Optimizasyon | WebOptimizer (bundle + minify), Brotli/Gzip sıkıştırma, `<picture>`/WebP + lazy-loading |
| E-posta | MailKit (SMTP) — `IEmailSender` arkasında soyutlanmış |
| Anti-bot | Google reCAPTCHA v3 + honeypot + rate limiting |
| Test | xUnit + Moq + FluentAssertions |

---

## 2. Çözüm Mimarisi

```
SkyOS.sln
├── src/
│   ├── SkyOS.Web/            → Sunum katmanı (Controllers, Views, wwwroot, Program.cs, Middleware)
│   ├── SkyOS.Application/    → İş mantığı, servisler, DTO'lar, interface'ler, validators, mappings
│   ├── SkyOS.Domain/         → Entity'ler, enum'lar, domain exceptions (saf POCO, bağımlılığı yok)
│   ├── SkyOS.Infrastructure/ → EF Core DbContext, Configurations, Repositories, Migrations, SMTP, reCAPTCHA
│   └── SkyOS.Shared/         → Result<T> pattern, sabitler, extension'lar
├── tests/
│   ├── SkyOS.Application.Tests/
│   └── SkyOS.Infrastructure.Tests/
├── .editorconfig
├── Directory.Build.props
└── README.md
```

**Bağımlılık yönü (sıkı):**

```
Web  →  Application  →  Domain
 └───→  Infrastructure  →  Domain
                └──→ Application (interface'ler burada; Infrastructure implemente eder)
Shared → herkes referans alabilir, kimseye bağımlı değildir.
```

- `Domain` **hiçbir** projeye referans vermez (framework-agnostic).
- Interface'ler `Application` içinde tanımlanır; somut implementasyonlar `Infrastructure`'dadır (Dependency Inversion).
- Controller'lar yalnızca `Application` interface'lerine bağımlıdır; `Infrastructure`'a yalnızca `Program.cs` içindeki DI kaydı erişir.

---

## 3. Ön Gereksinimler

- [.NET SDK 9.0+](https://dotnet.microsoft.com/download)
- SQL Server (Production için) — ya da yerel geliştirmede sıfır kurulumla **SQLite**.
- (Opsiyonel) `dotnet-ef` aracı: `dotnet tool install --global dotnet-ef`

---

## 4. Hızlı Başlangıç (Geliştirme — SQLite)

Geliştirme ortamı varsayılan olarak **SQLite** kullanır ve şema/örnek veri uygulama açılışında otomatik oluşturulur (`EnsureCreated`). Ek kuruluma gerek yoktur.

```bash
dotnet restore
dotnet build
dotnet run --project src/SkyOS.Web --launch-profile https
```

Uygulama şu adreslerde çalışır:

- https://localhost:7022
- http://localhost:5086

`appsettings.Development.json` içinde `Database:Provider = "Sqlite"` ve `ConnectionStrings:DefaultConnection = "Data Source=skyos.dev.db"` tanımlıdır. reCAPTCHA geliştirmede kapalıdır (`Recaptcha:Enabled = false`), böylece iletişim formu anahtarsız test edilebilir.

---

## 5. Production Kurulumu (SQL Server + Migrations)

1. **Ortamı ayarlayın:** `ASPNETCORE_ENVIRONMENT=Production`
2. **Bağlantı dizesi ve gizli anahtarları environment variable / Key Vault üzerinden** verin (aşağıya bakın).
3. Migration'ları uygulayın:

```bash
# EF komutları Infrastructure projesinde, startup olarak Web ile çalışır:
dotnet ef database update --project src/SkyOS.Infrastructure --startup-project src/SkyOS.Web
```

Uygulama SQL Server sağlayıcısında açılışta `Migrate()` çağırır; SQLite'ta ise `EnsureCreated()` kullanır (`DatabaseInitializer`).

### Migration komutları

```bash
# Yeni migration ekle
dotnet ef migrations add <Ad> --project src/SkyOS.Infrastructure --startup-project src/SkyOS.Web --output-dir Persistence/Migrations

# Veritabanına uygula
dotnet ef database update --project src/SkyOS.Infrastructure --startup-project src/SkyOS.Web

# Son migration'ı geri al
dotnet ef migrations remove --project src/SkyOS.Infrastructure --startup-project src/SkyOS.Web
```

> Migration'lar **SQL Server** hedefiyle üretilmiştir. `dotnet ef` tasarım-zamanı için `SkyOSDbContextFactory` kullanılır; bağlantı dizesi `SKYOS_MIGRATION_CONNECTION` ortam değişkeni ile geçersiz kılınabilir.

---

## 6. Gizli Anahtar Yönetimi (Secrets)

`appsettings.json`'a **asla** SMTP şifresi, reCAPTCHA secret key veya production connection string yazılmaz.

### Geliştirmede — `dotnet user-secrets`

```bash
cd src/SkyOS.Web
dotnet user-secrets set "Smtp:Password" "••••••"
dotnet user-secrets set "Recaptcha:SecretKey" "••••••"
dotnet user-secrets set "Recaptcha:SiteKey" "••••••"
dotnet user-secrets set "Recaptcha:Enabled" "true"
```

### Production'da — environment variables (çift alt çizgi hiyerarşiyi belirtir)

```
ConnectionStrings__DefaultConnection=Server=...;Database=SkyOS;...
Smtp__Host=smtp.example.com
Smtp__Username=no-reply@skyos.com
Smtp__Password=••••••
Smtp__FromAddress=no-reply@skyos.com
Recaptcha__Enabled=true
Recaptcha__SiteKey=••••••
Recaptcha__SecretKey=••••••
ContactForm__NotificationRecipientEmail=ekip@skyos.com
```

---

## 7. `appsettings.Development.json` Örneği (gizli anahtarlar hariç)

```jsonc
{
  "Serilog": {
    "MinimumLevel": { "Default": "Debug" }
  },
  "Database": { "Provider": "Sqlite" },
  "ConnectionStrings": { "DefaultConnection": "Data Source=skyos.dev.db" },
  "Recaptcha": { "Enabled": false },
  "ContactForm": { "NotificationRecipientEmail": "dev-inbox@example.com" }
}
```

### İletişim bilgileri (CMS/appsettings üzerinden yönetilir — hardcode yok)

Telefon ve e-posta `appsettings.json` içindeki `SiteContact` bölümünden gelir ve **kod değişikliği gerektirmeden** güncellenebilir:

```jsonc
"SiteContact": {
  "Email": "skyos.autonomy@gmail.com",   // placeholder — kurumsal domaine geçilmeli
  "PhonePrimary": "0533 499 0122",         // placeholder
  "PhoneSecondary": "0506 544 8944",       // placeholder
  "ContactDetailsArePlaceholder": true,
  "Social": { "LinkedIn": "", "X": "", "GitHub": "", "YouTube": "" }
}
```

Ekip üyeleri ve destekçiler veritabanından (CMS gibi) dinamik çekilir; yeni kayıt eklemek kod değişikliği gerektirmez.

---

## 8. Güvenlik (Bölüm 4 gereksinimleri)

| Kontrol | Uygulama |
| --- | --- |
| HTTPS / HSTS | `UseHttpsRedirection()` + `UseHsts()` (max-age 1 yıl, includeSubDomains, preload) |
| Security Headers | Özel `SecurityHeadersMiddleware` — CSP (nonce'lu), `X-Content-Type-Options`, `X-Frame-Options: DENY`, `Referrer-Policy`, `Permissions-Policy` |
| CSRF | Global `AutoValidateAntiforgeryToken` + form'larda `@Html.AntiForgeryToken()` + `[ValidateAntiForgeryToken]` |
| Input validasyonu | Server-side FluentValidation (otorite) + client-side unobtrusive (yalnızca UX) |
| Rate limiting | Built-in `RateLimiter` — iletişim formu IP başına 5 istek/dakika |
| SQL injection | Yalnızca EF Core LINQ; string birleştirme ile ham SQL **yok** |
| XSS | Razor otomatik encode; `Html.Raw` kullanıcı verisi için **kullanılmaz** |
| Bot/Spam | reCAPTCHA v3 + honeypot + servis düzeyinde IP throttle |
| Secrets | user-secrets (dev) / environment variables (prod) |
| Hata yönetimi | Prod'da `UseExceptionHandler("/hata/500")` + `UseStatusCodePagesWithReExecute` + özel 404/500 |
| PII loglama | Serilog loglarında e-posta/telefon maskelenir (`MaskEmail`, `MaskPhone`) |

**Güvenlik başlıklarını doğrulama:**

```bash
curl -k -I https://localhost:7022/
```

---

## 9. Performans (Bölüm 5 gereksinimleri)

- **Bundle + minify:** WebOptimizer ile `/css/site.min.css` ve `/js/site.min.js`.
- **Sıkıştırma:** Brotli + Gzip (`UseResponseCompression`).
- **Caching:** Statik varlıklar için `Cache-Control: public,max-age=1yıl,immutable` + `asp-append-version` ile cache-busting.
- **Fontlar:** Sistem font yığını (sıfır ağ isteği; harici CDN yok). Inter/Space Grotesk `.woff2` dosyaları `wwwroot/fonts` altına eklenip `site.css` içinde `@font-face` ile self-host edilebilir.
- **Görseller:** `<picture>` + WebP + `width/height` (CLS önleme) + `loading="lazy"`.
- **SSR:** Razor sunucu tarafı; SPA yok, JS bağımlılığı minimumda.

---

## 10. Veritabanı Şeması

| Tablo | Alanlar |
| --- | --- |
| `TeamMembers` | Id, FullName, Role, Bio, PhotoUrl, DisplayOrder, IsLeader |
| `Partners` | Id, Name, LogoUrl, Description, WebsiteUrl, DisplayOrder, IsVerifiedRelationship |
| `Milestones` | Id, Title, Description, DateAchieved, Category (enum) |
| `ContactMessages` | Id, FullName, Company, Email, Phone, InterestType (enum), Message, CreatedAtUtc, IsRead, IpAddress |
| `NewsItems` | Id, Title, Slug, Summary, Body, PublishedAtUtc, IsPublished |

Tüm entity'ler `BaseEntity` (Id, CreatedAtUtc, UpdatedAtUtc) türetir. Konfigürasyon Fluent API ile (`IEntityTypeConfiguration<T>`), attribute tabanlı değildir. Seed veri: 2 ekip lideri, HAVELSAN (doğrulanmış), 5 kilometre taşı.

---

## 11. Test

```bash
dotnet test
```

- **Application.Tests:** `ContactMessageService` (honeypot, reCAPTCHA reddi, throttle, e-posta hatasında bile kalıcılık) ve `ContactMessageValidator`.
- **Infrastructure.Tests:** `GenericRepository` / `UnitOfWork` — SQLite in-memory üzerinde gerçek EF pipeline, seed veri ve audit timestamp doğrulaması.

---

## 12. Sayfalar

| Rota | Sayfa |
| --- | --- |
| `/` | Ana Sayfa (hero, istatistikler, 4 özellik, HAVELSAN doğrulama bandı) |
| `/hakkimizda` | Hakkımızda (misyon, problem/çözüm) |
| `/urun` | Ürün / SkyOS Nedir (geliştirme durumu tablosu) |
| `/kullanim-alanlari` | Kullanım Alanları (5 segment) |
| `/basarilar` | Başarılar ve Doğrulama (TEKNOFEST, HAVELSAN, pazar öngörüsü) |
| `/destekcilerimiz` | Destekçilerimiz (logo-grid, dinamik) |
| `/yatirimcilar` | Yatırımcı İlişkileri (öncelikler, katkı tablosu, CTA) |
| `/ekip` | Ekip (dinamik, CMS) |
| `/iletisim` | İletişim (güvenli form) — `?ilgi=yatirimci` varyantı |
| `/gizlilik-politikasi` | KVKK / Gizlilik (hukuki şablon) |
| `/hata/{code}` | Özel 404 / 500 |

---

## 13. Kod Prensipleri

- **SOLID** (tek sorumluluk servisler, küçük amaca özel interface'ler, DI ile gevşek bağlılık).
- Tüm I/O `async/await` + `CancellationToken`.
- Servis katmanı exception yerine **`Result<T>`** döner.
- Nullable reference types açık, `ImplicitUsings` açık.
- `.NET analyzers` + `.editorconfig` ile stil zorlaması; `dotnet format` uyumlu.

### Bağımlılık güvenliği (CI önerisi)

```bash
dotnet list package --vulnerable --include-transitive
```

---

## 14. Yasal Notlar

- HAVELSAN adı yalnızca **metinsel** olarak kullanılır; logo kullanımı hukuki onay gerektirir.
- KVKK/Gizlilik metni bir **şablondur** ve yayına alınmadan önce hukuk danışmanı onayı gerektirir. Bu depo hukuki tavsiye içermez.
- İletişim bilgileri (e-posta, telefon) **placeholder**'dır; canlıya almadan önce güncellenmelidir.
