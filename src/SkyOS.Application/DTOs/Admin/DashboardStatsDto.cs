namespace SkyOS.Application.DTOs.Admin;

public sealed class DashboardStatsDto
{
    public int UnreadContactMessages { get; set; }

    public int UnreadSiteFeedbacks { get; set; }

    public int TotalContactMessages { get; set; }

    public int TotalSiteFeedbacks { get; set; }

    public int AuditLogsToday { get; set; }
}
