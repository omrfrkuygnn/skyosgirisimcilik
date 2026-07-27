using SkyOS.Application.DTOs.Feedback;
using SkyOS.Shared.Results;

namespace SkyOS.Application.Interfaces.Services;

public interface ISiteFeedbackService
{
    Task<Result<SiteFeedbackResponseDto>> SubmitAsync(SiteFeedbackRequestDto request, CancellationToken cancellationToken = default);
}
