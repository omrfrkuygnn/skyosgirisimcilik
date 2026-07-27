using FluentValidation;
using SkyOS.Application.DTOs.Feedback;
using SkyOS.Shared.Localization;

namespace SkyOS.Application.Validators;

public sealed class SiteFeedbackValidator : AbstractValidator<SiteFeedbackRequestDto>
{
    public SiteFeedbackValidator(IAppLocalizer localizer)
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage(localizer["FeedbackValidation.FullNameRequired"])
            .MaximumLength(120).WithMessage(localizer["FeedbackValidation.FullNameMax"]);

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage(localizer["FeedbackValidation.EmailInvalid"])
            .MaximumLength(254).WithMessage(localizer["FeedbackValidation.EmailMax"]);

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage(localizer["FeedbackValidation.MessageRequired"])
            .MinimumLength(10).WithMessage(localizer["FeedbackValidation.MessageMin"])
            .MaximumLength(4000).WithMessage(localizer["FeedbackValidation.MessageMax"]);
    }
}
