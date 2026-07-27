using SkyOS.Application.DTOs.Admin;
using SkyOS.Application.Interfaces.Infrastructure;
using SkyOS.Application.Interfaces.Persistence;
using SkyOS.Application.Interfaces.Services;
using SkyOS.Domain.Entities;

namespace SkyOS.Application.Services;

public sealed class DashboardAdminService : IDashboardAdminService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public DashboardAdminService(IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider)
    {
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<DashboardStatsDto> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var contactRepo = _unitOfWork.Repository<ContactMessage>();
        var feedbackRepo = _unitOfWork.Repository<SiteFeedback>();
        var auditRepo = _unitOfWork.Repository<AuditLog>();

        var today = _dateTimeProvider.UtcNow.Date;

        return new DashboardStatsDto
        {
            UnreadContactMessages = await contactRepo.CountAsync(x => !x.IsRead, cancellationToken).ConfigureAwait(false),
            UnreadSiteFeedbacks = await feedbackRepo.CountAsync(x => !x.IsRead, cancellationToken).ConfigureAwait(false),
            TotalContactMessages = await contactRepo.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false),
            TotalSiteFeedbacks = await feedbackRepo.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false),
            AuditLogsToday = await auditRepo.CountAsync(x => x.CreatedAtUtc >= today, cancellationToken).ConfigureAwait(false),
        };
    }
}
