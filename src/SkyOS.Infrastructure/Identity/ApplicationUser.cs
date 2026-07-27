using Microsoft.AspNetCore.Identity;

namespace SkyOS.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
