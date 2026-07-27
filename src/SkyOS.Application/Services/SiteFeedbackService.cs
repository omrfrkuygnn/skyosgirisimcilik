using FluentValidation;
using Mapster;
using Microsoft.Extensions.Logging;
using SkyOS.Application.DTOs.Feedback;
using SkyOS.Application.Interfaces.Infrastructure;
using SkyOS.Application.Interfaces.Persistence;
using SkyOS.Application.Interfaces.Services;
using SkyOS.Domain.Entities;
using SkyOS.Shared.Results;

namespace SkyOS.Application.Services;

public sealed class SiteFeedbackService : ISiteFeedbackService
{
    private const string RecaptchaAction = "feedback";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IRecaptchaValidator _recaptchaValidator;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<SiteFeedbackService> _logger;

    public SiteFeedbackService(
        IUnitOfWork unitOfWork,
        IRecaptchaValidator recaptchaValidator,
        IDateTimeProvider dateTimeProvider,
        ILogger<SiteFeedbackService> logger)
    {
        _unitOfWork = unitOfWork;
        _recaptchaValidator = recaptchaValidator;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task<Result<SiteFeedbackResponseDto>> SubmitAsync(
        SiteFeedbackRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(request.Website))
        {
            _logger.LogWarning("Feedback honeypot triggered from IP {Ip}.", request.IpAddress);
            return Result.Success(new SiteFeedbackResponseDto
            {
                CreatedAtUtc = _dateTimeProvider.UtcNow,
            });
        }

        var recaptcha = await _recaptchaValidator
            .ValidateAsync(request.RecaptchaToken, RecaptchaAction, request.IpAddress, cancellationToken)
            .ConfigureAwait(false);

        if (recaptcha.IsFailure)
        {
            return Result.Failure<SiteFeedbackResponseDto>(recaptcha.Error);
        }

        var entity = request.Adapt<SiteFeedback>();
        entity.IpAddress = request.IpAddress;
        entity.IsRead = false;
        entity.Culture = string.IsNullOrWhiteSpace(request.Culture) ? "tr" : request.Culture;

        await _unitOfWork.Repository<SiteFeedback>().AddAsync(entity, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Site feedback #{Id} stored (category {Category}).", entity.Id, entity.Category);

        return Result.Success(entity.Adapt<SiteFeedbackResponseDto>());
    }
}
