using FluentAssertions;
using SkyOS.Application.DTOs.Contact;
using SkyOS.Application.Validators;
using SkyOS.Domain.Enums;
using SkyOS.Shared.Localization;

namespace SkyOS.Application.Tests.Validators;

public sealed class ContactMessageValidatorTests
{
    private readonly ContactMessageValidator _validator = new(new StubLocalizer());

    private static ContactMessageRequestDto Valid() => new()
    {
        FullName = "Ada Lovelace",
        Email = "ada@example.com",
        InterestType = InterestType.KurumsalIsBirligi,
        Message = "Kurumsal iş birliği hakkında görüşmek isterim.",
    };

    [Fact]
    public void Validate_WithValidModel_Passes()
    {
        var result = _validator.Validate(Valid());
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Validate_WithInvalidEmail_Fails(string email)
    {
        var model = Valid();
        model.Email = email;

        var result = _validator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ContactMessageRequestDto.Email));
    }

    [Fact]
    public void Validate_WithShortMessage_Fails()
    {
        var model = Valid();
        model.Message = "kısa";

        var result = _validator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ContactMessageRequestDto.Message));
    }

    [Fact]
    public void Validate_WithInvalidPhoneCharacters_Fails()
    {
        var model = Valid();
        model.Phone = "abc-not-phone";

        var result = _validator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ContactMessageRequestDto.Phone));
    }

    [Fact]
    public void Validate_WithMissingFullName_Fails()
    {
        var model = Valid();
        model.FullName = "";

        var result = _validator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ContactMessageRequestDto.FullName));
    }

    private sealed class StubLocalizer : IAppLocalizer
    {
        public string Culture => "tr";
        public string this[string key] => key;
        public string this[string key, params object[] args] => key;
        public string Get(string key, params object[] args) => key;
    }
}
