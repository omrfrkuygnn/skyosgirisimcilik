using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SkyOS.Infrastructure.Identity;
using SkyOS.Infrastructure.Options;

namespace SkyOS.Infrastructure.Identity;

public static class IdentitySeeder
{
    public const string AdminRole = "Admin";

    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("IdentitySeeder");
        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var options = provider.GetRequiredService<IOptions<BackofficeOptions>>().Value;

        if (!await roleManager.RoleExistsAsync(AdminRole).ConfigureAwait(false))
        {
            await roleManager.CreateAsync(new IdentityRole(AdminRole)).ConfigureAwait(false);
        }

        var admin = await userManager.FindByEmailAsync(options.DefaultAdminEmail).ConfigureAwait(false);
        if (admin is not null)
        {
            return;
        }

        admin = new ApplicationUser
        {
            UserName = options.DefaultAdminEmail,
            Email = options.DefaultAdminEmail,
            EmailConfirmed = true,
            DisplayName = options.DefaultAdminDisplayName,
            IsActive = true,
        };

        var result = await userManager.CreateAsync(admin, options.DefaultAdminPassword).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogError("Failed to seed default admin user: {Errors}", errors);
            return;
        }

        await userManager.AddToRoleAsync(admin, AdminRole).ConfigureAwait(false);
        logger.LogInformation("Default backoffice admin user seeded ({Email}).", options.DefaultAdminEmail);
    }
}
