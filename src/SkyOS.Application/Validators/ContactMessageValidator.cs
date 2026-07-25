using FluentValidation;
using SkyOS.Application.DTOs.Contact;
using SkyOS.Shared.Localization;

namespace SkyOS.Application.Validators;

/// <summary>
/// Server-side validation for the contact form. This is the authoritative check;
/// client-side unobtrusive validation only improves UX and is never trusted alone.
/// </summary>
public sealed class ContactMessageValidator : AbstractValidator<ContactMessageRequestDto>
{
    public ContactMessageValidator(IAppLocalizer L)
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage(_ => L["Validation.FullNameRequired"])
            .MaximumLength(120).WithMessage(_ => L["Validation.FullNameMax"]);

        RuleFor(x => x.Company)
            .MaximumLength(160).WithMessage(_ => L["Validation.CompanyMax"]);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(_ => L["Validation.EmailRequired"])
            .EmailAddress().WithMessage(_ => L["Validation.EmailInvalid"])
            .MaximumLength(254).WithMessage(_ => L["Validation.EmailMax"]);

        RuleFor(x => x.Phone)
            .MaximumLength(30).WithMessage(_ => L["Validation.PhoneMax"])
            .Matches(@"^[0-9\s()+\-]*$").WithMessage(_ => L["Validation.PhoneFormat"])
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.InterestType)
            .IsInEnum().WithMessage(_ => L["Validation.InterestInvalid"]);

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage(_ => L["Validation.MessageRequired"])
            .MinimumLength(10).WithMessage(_ => L["Validation.MessageMin"])
            .MaximumLength(4000).WithMessage(_ => L["Validation.MessageMax"]);
    }
}
