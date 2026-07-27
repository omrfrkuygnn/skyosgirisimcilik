using SkyOS.Application.DTOs.Admin;

namespace SkyOS.Application.Interfaces.Services;

public interface IDashboardAdminService
{
    Task<DashboardStatsDto> GetStatsAsync(CancellationToken cancellationToken = default);
}
