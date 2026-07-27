using SkyOS.Application.DTOs.Admin;
using SkyOS.Shared.Results;

namespace SkyOS.Application.Interfaces.Services;

public interface IAdminReplyService
{
    Task<Result> ReplyToContactAsync(
        int contactMessageId,
        AdminReplyDto reply,
        string adminEmail,
        string? adminDisplayName,
        CancellationToken cancellationToken = default);

    Task<Result> ReplyToFeedbackAsync(
        int feedbackId,
        AdminReplyDto reply,
        string adminEmail,
        string? adminDisplayName,
        CancellationToken cancellationToken = default);
}
