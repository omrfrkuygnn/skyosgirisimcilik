using SkyOS.Application.Interfaces.Infrastructure;

namespace SkyOS.Infrastructure.Services;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
