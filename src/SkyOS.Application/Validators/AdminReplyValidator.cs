using FluentValidation;
using SkyOS.Application.DTOs.Admin;

namespace SkyOS.Application.Validators;

public sealed class AdminReplyValidator : AbstractValidator<AdminReplyDto>
{
    public AdminReplyValidator()
    {
        RuleFor(x => x.Subject)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Message)
            .NotEmpty()
            .MinimumLength(10)
            .MaximumLength(4000);
    }
}
