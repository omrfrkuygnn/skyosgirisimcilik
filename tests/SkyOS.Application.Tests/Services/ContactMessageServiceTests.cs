using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using SkyOS.Application.DTOs.Common;
using SkyOS.Application.DTOs.Contact;
using SkyOS.Application.Interfaces.Infrastructure;
using SkyOS.Application.Interfaces.Persistence;
using SkyOS.Application.Options;
using SkyOS.Application.Services;
using SkyOS.Domain.Entities;
using SkyOS.Domain.Enums;
using SkyOS.Shared.Localization;
using SkyOS.Shared.Results;

namespace SkyOS.Application.Tests.Services;

public sealed class ContactMessageServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IGenericRepository<ContactMessage>> _repository = new();
    private readonly Mock<IEmailSender> _emailSender = new();
    private readonly Mock<IRecaptchaValidator> _recaptcha = new();
    private readonly Mock<IDateTimeProvider> _clock = new();
    private readonly Mock<IAppLocalizer> _localizer = new();
    private readonly ContactFormOptions _options = new()
    {
        NotificationRecipientEmail = "team@skyos.example",
        MaxSubmissionsPerWindow = 5,
        SpamWindowMinutes = 10,
    };

    public ContactMessageServiceTests()
    {
        _unitOfWork.Setup(u => u.Repository<ContactMessage>()).Returns(_repository.Object);
        _clock.SetupGet(c => c.UtcNow).Returns(new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc));
        _recaptcha
            .Setup(r => r.ValidateAsync(It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        _emailSender
            .Setup(e => e.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        _repository
            .Setup(r => r.CountAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ContactMessage, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        _localizer.Setup(l => l[It.IsAny<string>()]).Returns((string key) => key);
        _localizer.Setup(l => l.Get(It.IsAny<string>(), It.IsAny<object[]>())).Returns((string key, object[] _) => key);
    }

    private ContactMessageService CreateSut() => new(
        _unitOfWork.Object,
        _emailSender.Object,
        _recaptcha.Object,
        _clock.Object,
        Microsoft.Extensions.Options.Options.Create(_options),
        _localizer.Object,
        NullLogger<ContactMessageService>.Instance);

    private static ContactMessageRequestDto ValidRequest() => new()
    {
        FullName = "Ada Lovelace",
        Email = "ada@example.com",
        InterestType = InterestType.Yatirimci,
        Message = "Yatırım görüşmesi talep ediyorum.",
        IpAddress = "203.0.113.5",
    };

    [Fact]
    public async Task SubmitAsync_WithValidRequest_PersistsAndNotifies()
    {
        var sut = CreateSut();

        var result = await sut.SubmitAsync(ValidRequest());

        result.IsSuccess.Should().BeTrue();
        _repository.Verify(r => r.AddAsync(It.IsAny<ContactMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _emailSender.Verify(e => e.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SubmitAsync_WhenHoneypotFilled_DiscardsSilently()
    {
        var request = ValidRequest();
        request.Website = "http://spam.example"; // bot filled the hidden field

        var sut = CreateSut();
        var result = await sut.SubmitAsync(request);

        result.IsSuccess.Should().BeTrue(); // we do not reveal the trap
        _repository.Verify(r => r.AddAsync(It.IsAny<ContactMessage>(), It.IsAny<CancellationToken>()), Times.Never);
        _emailSender.Verify(e => e.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SubmitAsync_WhenRecaptchaFails_ReturnsFailureAndDoesNotPersist()
    {
        _recaptcha
            .Setup(r => r.ValidateAsync(It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(Error.Validation("captcha")));

        var sut = CreateSut();
        var result = await sut.SubmitAsync(ValidRequest());

        result.IsFailure.Should().BeTrue();
        _repository.Verify(r => r.AddAsync(It.IsAny<ContactMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SubmitAsync_WhenThrottleExceeded_ReturnsTooManyRequests()
    {
        _repository
            .Setup(r => r.CountAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ContactMessage, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_options.MaxSubmissionsPerWindow);

        var sut = CreateSut();
        var result = await sut.SubmitAsync(ValidRequest());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("General.TooManyRequests");
        _repository.Verify(r => r.AddAsync(It.IsAny<ContactMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SubmitAsync_WhenEmailFails_StillSucceedsBecauseMessageIsStored()
    {
        _emailSender
            .Setup(e => e.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(Error.Failure("smtp down")));

        var sut = CreateSut();
        var result = await sut.SubmitAsync(ValidRequest());

        result.IsSuccess.Should().BeTrue();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
