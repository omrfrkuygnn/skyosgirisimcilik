using Microsoft.Extensions.Options;
using SkyOS.Infrastructure.Options;

namespace SkyOS.Web.Helpers;

public static class RecaptchaUi
{
    public static bool IsEnabled(IOptions<RecaptchaOptions> options) =>
        options.Value.Enabled && options.Value.HasConfiguredKeys;
}
