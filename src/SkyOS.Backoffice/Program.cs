using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Serilog;
using SkyOS.Application;
using SkyOS.Application.Options;
using SkyOS.Infrastructure;
using SkyOS.Infrastructure.Identity;
using SkyOS.Infrastructure.Persistence;
using SkyOS.Backoffice.Extensions;
using SkyOS.Backoffice.Options;
using SkyOS.Backoffice.Services;

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

    builder.Services.AddApplication();
    builder.Services.Configure<ContactFormOptions>(
        builder.Configuration.GetSection(ContactFormOptions.SectionName));
    builder.Services.Configure<ContentUploadOptions>(
        builder.Configuration.GetSection(ContentUploadOptions.SectionName));
    builder.Services.AddSingleton<IContentUploadService, ContentUploadService>();
    builder.Services.Configure<FormOptions>(options =>
    {
        options.MultipartBodyLengthLimit = 15 * 1024 * 1024;
    });
    builder.Services.AddInfrastructure(builder.Configuration, builder.Environment.ContentRootPath);
    builder.Services.AddSkyOsLocalization();

    builder.Services.AddControllersWithViews(options =>
    {
        options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequireRole(IdentitySeeder.AdminRole)
            .Build();
        options.Filters.Add(new AuthorizeFilter(policy));
    });

    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddFluentValidationClientsideAdapters();
    builder.Services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN");

    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

    var app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Account/Login");
        app.UseHsts();
    }

    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }
    app.UseStaticFiles();
    app.UseRouting();
    app.UseSkyOsLocalization();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Dashboard}/{action=Index}/{id?}");

    await DatabaseInitializer.InitializeAsync(app.Services);

    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "SkyOS Backoffice host terminated unexpectedly.");
}
finally
{
    await Log.CloseAndFlushAsync();
}
