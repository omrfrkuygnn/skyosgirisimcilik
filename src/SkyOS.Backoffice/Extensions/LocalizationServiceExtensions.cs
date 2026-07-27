using System.Globalization;
using Microsoft.AspNetCore.Localization;
using SkyOS.Shared.Localization;
using SkyOS.Backoffice.Localization;

namespace SkyOS.Backoffice.Extensions;

public static class LocalizationServiceExtensions
{
    public const string CookieName = "SkyOS.Culture";
    public const string QueryKey = "lang";

    public static readonly string[] SupportedCultures = ["tr", "en", "de"];

    public static IServiceCollection AddSkyOsLocalization(this IServiceCollection services)
    {
        services.AddSingleton<LocaleCatalog>();
        services.AddScoped<IAppLocalizer, JsonAppLocalizer>();

        services.Configure<RequestLocalizationOptions>(options =>
        {
            var cultures = SupportedCultures.Select(c => new CultureInfo(c)).ToList();
            options.DefaultRequestCulture = new RequestCulture("tr");
            options.SupportedCultures = cultures;
            options.SupportedUICultures = cultures;
            options.ApplyCurrentCultureToResponseHeaders = true;
            options.RequestCultureProviders =
            [
                new CookieRequestCultureProvider { CookieName = CookieName },
                new QueryStringRequestCultureProvider { QueryStringKey = QueryKey, UIQueryStringKey = QueryKey },
                new AcceptLanguageHeaderRequestCultureProvider(),
            ];
        });

        return services;
    }

    public static IApplicationBuilder UseSkyOsLocalization(this IApplicationBuilder app)
        => app.UseRequestLocalization();
}
