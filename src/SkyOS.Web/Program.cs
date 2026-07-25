using System.IO.Compression;
using System.Threading.RateLimiting;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Net.Http.Headers;
using Serilog;
using WebOptimizer;
using SkyOS.Application;
using SkyOS.Application.Options;
using SkyOS.Infrastructure;
using SkyOS.Infrastructure.Persistence;
using SkyOS.Shared.Constants;
using SkyOS.Web.Extensions;
using SkyOS.Web.Middleware;
using SkyOS.Web.Options;

// Two-stage Serilog init: a bootstrap logger captures failures during startup itself.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // ---- Options (contact info & form config are editable via appsettings, never hardcoded) ----
    builder.Services.Configure<SiteContactOptions>(
        builder.Configuration.GetSection(SiteContactOptions.SectionName));
    builder.Services.Configure<ContactFormOptions>(
        builder.Configuration.GetSection(ContactFormOptions.SectionName));

    // ---- Layers ----
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddSkyOsLocalization();

    // ---- MVC + anti-forgery + validation ----
    builder.Services.AddControllersWithViews(options =>
    {
        // Defence in depth: validate the anti-forgery token on every unsafe (POST/PUT/DELETE) request.
        options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    });

    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddFluentValidationClientsideAdapters();

    builder.Services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN");

    // ---- Static asset bundling + minification (no external CDNs) ----
    builder.Services.AddWebOptimizer(pipeline =>
    {
        pipeline.AddCssBundle("/css/site.min.css", "css/site.v3.css");
        pipeline.AddJavaScriptBundle("/js/site.min.js", "js/site.js");
    });

    // ---- Response compression (Brotli + Gzip) ----
    builder.Services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
        options.Providers.Add<BrotliCompressionProvider>();
        options.Providers.Add<GzipCompressionProvider>();
        options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        [
            "image/svg+xml",
            "application/json",
        ]);
    });
    builder.Services.Configure<BrotliCompressionProviderOptions>(o => o.Level = CompressionLevel.Optimal);
    builder.Services.Configure<GzipCompressionProviderOptions>(o => o.Level = CompressionLevel.Optimal);

    // ---- HSTS (min 1 year) ----
    builder.Services.AddHsts(options =>
    {
        options.MaxAge = TimeSpan.FromDays(365);
        options.IncludeSubDomains = true;
        options.Preload = true;
    });

    // ---- Rate limiting: contact form = 5 requests / minute / IP ----
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.AddPolicy(AppConstants.RateLimiting.ContactFormPolicy, httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = AppConstants.RateLimiting.ContactFormPermitPerWindow,
                    Window = TimeSpan.FromSeconds(AppConstants.RateLimiting.ContactFormWindowSeconds),
                    QueueLimit = 0,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                }));
    });

    var app = builder.Build();

    // ---------------- Middleware pipeline (order matters) ----------------
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/hata/500");
        app.UseStatusCodePagesWithReExecute("/hata/{0}");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseSkyOsLocalization();
    app.UseResponseCompression();

    // Security headers (incl. CSP nonce) applied to every response, including static assets.
    app.UseMiddleware<SecurityHeadersMiddleware>();

    app.UseWebOptimizer();

    app.UseStaticFiles(new StaticFileOptions
    {
        OnPrepareResponse = ctx =>
        {
            ctx.Context.Response.Headers[HeaderNames.CacheControl] =
                $"public,max-age={AppConstants.Cache.StaticAssetMaxAgeSeconds},immutable";
        },
    });

    app.UseRouting();
    app.UseRateLimiter();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    // Apply schema/seed on startup (Migrate for SqlServer, EnsureCreated for the Sqlite dev fallback).
    await DatabaseInitializer.InitializeAsync(app.Services);

    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "SkyOS host terminated unexpectedly.");
}
finally
{
    await Log.CloseAndFlushAsync();
}
