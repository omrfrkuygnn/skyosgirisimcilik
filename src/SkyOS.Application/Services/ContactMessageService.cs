using Mapster;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SkyOS.Application.DTOs.Common;
using SkyOS.Application.DTOs.Contact;
using SkyOS.Application.Interfaces.Infrastructure;
using SkyOS.Application.Interfaces.Persistence;
using SkyOS.Application.Interfaces.Services;
using SkyOS.Application.Options;
using SkyOS.Domain.Entities;
using SkyOS.Domain.Enums;
using SkyOS.Shared.Extensions;
using SkyOS.Shared.Localization;
using SkyOS.Shared.Results;

namespace SkyOS.Application.Services;

public sealed class ContactMessageService : IContactMessageService
{
    private const string RecaptchaAction = "contact";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailSender _emailSender;
    private readonly IRecaptchaValidator _recaptchaValidator;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ContactFormOptions _options;
    private readonly IAppLocalizer _L;
    private readonly ILogger<ContactMessageService> _logger;

    public ContactMessageService(
        IUnitOfWork unitOfWork,
        IEmailSender emailSender,
        IRecaptchaValidator recaptchaValidator,
        IDateTimeProvider dateTimeProvider,
        IOptions<ContactFormOptions> options,
        IAppLocalizer localizer,
        ILogger<ContactMessageService> logger)
    {
        _unitOfWork = unitOfWork;
        _emailSender = emailSender;
        _recaptchaValidator = recaptchaValidator;
        _dateTimeProvider = dateTimeProvider;
        _options = options.Value;
        _L = localizer;
        _logger = logger;
    }

    public async Task<Result<ContactMessageResponseDto>> SubmitAsync(
        ContactMessageRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // 1) Honeypot: a filled hidden field means a bot. Pretend success so we do not
        //    reveal the trap, but persist/notify nothing.
        if (!string.IsNullOrWhiteSpace(request.Website))
        {
            _logger.LogWarning("Contact honeypot triggered from IP {Ip}. Submission discarded.", request.IpAddress);
            return Result.Success(new ContactMessageResponseDto
            {
                FullName = request.FullName,
                CreatedAtUtc = _dateTimeProvider.UtcNow,
            });
        }

        // 2) reCAPTCHA v3 server-side verification.
        var recaptcha = await _recaptchaValidator
            .ValidateAsync(request.RecaptchaToken, RecaptchaAction, request.IpAddress, cancellationToken)
            .ConfigureAwait(false);

        if (recaptcha.IsFailure)
        {
            _logger.LogWarning("Contact reCAPTCHA rejected from IP {Ip}: {Error}", request.IpAddress, recaptcha.Error.Code);
            return Result.Failure<ContactMessageResponseDto>(recaptcha.Error);
        }

        // 3) Per-IP throttle as defence-in-depth on top of the middleware rate limiter.
        var throttle = await EnsureNotSpammingAsync(request.IpAddress, cancellationToken).ConfigureAwait(false);
        if (throttle.IsFailure)
        {
            return Result.Failure<ContactMessageResponseDto>(throttle.Error);
        }

        // 4) Persist with formatted phone number including country code.
        var fullPhone = string.IsNullOrWhiteSpace(request.Phone)
            ? null
            : request.Phone.Trim().StartsWith("+")
                ? request.Phone.Trim()
                : $"{request.PhoneCountryCode.Trim()} {request.Phone.Trim()}";

        var entity = request.Adapt<ContactMessage>();
        entity.Phone = fullPhone;
        entity.IpAddress = request.IpAddress;
        entity.IsRead = false;

        await _unitOfWork.Repository<ContactMessage>().AddAsync(entity, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Contact message #{Id} stored (email {Email}, interest {Interest}).",
            entity.Id,
            entity.Email.MaskEmail(),
            entity.InterestType);

        // 5) Notify the team. A transport failure must not fail the visitor's request —
        //    the message is already safely stored.
        await NotifyTeamAsync(entity, request.Culture, cancellationToken).ConfigureAwait(false);

        return Result.Success(entity.Adapt<ContactMessageResponseDto>());
    }

    private async Task<Result> EnsureNotSpammingAsync(string? ipAddress, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            return Result.Success();
        }

        var threshold = _dateTimeProvider.UtcNow.AddMinutes(-_options.SpamWindowMinutes);
        var recentCount = await _unitOfWork.Repository<ContactMessage>()
            .CountAsync(m => m.IpAddress == ipAddress && m.CreatedAtUtc >= threshold, cancellationToken)
            .ConfigureAwait(false);

        if (recentCount >= _options.MaxSubmissionsPerWindow)
        {
            _logger.LogWarning("Contact throttle hit for IP {Ip}: {Count} messages in window.", ipAddress, recentCount);
            return Result.Failure(Error.TooManyRequests(_L["Contact.ThrottleError"]));
        }

        return Result.Success();
    }

    private async Task NotifyTeamAsync(ContactMessage entity, string culture, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.NotificationRecipientEmail))
        {
            _logger.LogWarning("ContactForm.NotificationRecipientEmail is not configured; skipping notification.");
            return;
        }

        var labels = ContactEmailCopy.ForCulture(culture);
        var interestLabel = ContactEmailCopy.InterestLabel(entity.InterestType, culture);

        var subject = $"[{labels.SubjectPrefix}] {interestLabel} — {entity.FullName}";

        var email = new EmailMessage
        {
            Subject = subject,
            HtmlBody = BuildNotificationHtml(entity, labels, interestLabel),
            PlainTextBody = BuildNotificationText(entity, labels, interestLabel),
            ReplyToAddress = entity.Email,
            ReplyToName = entity.FullName,
        };

        var send = await _emailSender.SendAsync(email, cancellationToken).ConfigureAwait(false);
        if (send.IsFailure)
        {
            _logger.LogError("Failed to send contact notification for message #{Id}: {Error}", entity.Id, send.Error.Code);
        }
    }

    private static string BuildNotificationHtml(ContactMessage m, ContactEmailCopy.Labels labels, string interestLabel)
    {
        // Values are HTML-encoded to prevent injection into the notification e-mail body.
        static string enc(string? value) => System.Net.WebUtility.HtmlEncode(value ?? "-");

        var messageHtml = enc(m.Message).Replace("\n", "<br />");

        return $"""
            <!DOCTYPE html>
            <html lang="{labels.Lang}">
            <head>
              <meta charset="UTF-8" />
              <meta name="viewport" content="width=device-width, initial-scale=1.0" />
              <title>{labels.Heading}</title>
            </head>
            <body style="margin:0;padding:0;background-color:#f4f6f9;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;-webkit-font-smoothing:antialiased;">
              <table width="100%" cellpadding="0" cellspacing="0" style="background-color:#f4f6f9;padding:32px 16px;">
                <tr><td align="center">
                  
                  <!-- MAIN WRAPPER CARD -->
                  <table width="620" cellpadding="0" cellspacing="0" style="max-width:620px;width:100%;background-color:#ffffff;border-radius:12px;border:1px solid #e2e8f0;box-shadow:0 10px 25px -5px rgba(15,23,42,0.06);overflow:hidden;">
                    
                    <!-- BRAND HEADER -->
                    <tr>
                      <td style="background-color:#0f172a;padding:28px 36px;">
                        <table width="100%" cellpadding="0" cellspacing="0">
                          <tr>
                            <td>
                              <span style="font-size:24px;font-weight:800;color:#ffffff;letter-spacing:-0.5px;">Sky<span style="color:#0284c7;">OS</span></span>
                              <span style="display:inline-block;margin-left:10px;padding:4px 12px;background-color:rgba(2,132,199,0.25);border-radius:4px;font-size:11px;font-weight:700;color:#38bdf8;letter-spacing:0.8px;">{labels.BadgeHeading}</span>
                            </td>
                            <td align="right">
                              <span style="font-size:12px;font-weight:600;color:#94a3b8;letter-spacing:0.3px;">ORKA Mühendislik</span>
                            </td>
                          </tr>
                        </table>
                      </td>
                    </tr>

                    <!-- HERO / SUB-HEADER -->
                    <tr>
                      <td style="background-color:#f8fafc;padding:20px 36px;border-bottom:1px solid #e2e8f0;">
                        <table width="100%" cellpadding="0" cellspacing="0">
                          <tr>
                            <td>
                              <span style="font-size:11px;font-weight:700;color:#64748b;letter-spacing:1px;">{labels.LabelTopic}:</span>
                              <span style="display:inline-block;margin-left:6px;padding:4px 12px;background-color:#e0f2fe;border:1px solid #bae6fd;border-radius:16px;font-size:13px;font-weight:700;color:#0369a1;">{enc(interestLabel)}</span>
                            </td>
                            <td align="right" style="font-size:12px;color:#64748b;">
                              {labels.DateLabel}: <strong>{enc(m.CreatedAtUtc.ToString("dd.MM.yyyy HH:mm"))} UTC</strong>
                            </td>
                          </tr>
                        </table>
                      </td>
                    </tr>

                    <!-- FORM DATA GRID -->
                    <tr>
                      <td style="padding:32px 36px 16px;">
                        <table width="100%" cellpadding="0" cellspacing="0">
                          
                          <!-- NAME -->
                          <tr>
                            <td style="padding-bottom:18px;width:130px;vertical-align:top;font-size:13px;font-weight:600;color:#64748b;">{labels.LabelName}:</td>
                            <td style="padding-bottom:18px;vertical-align:top;font-size:15px;font-weight:700;color:#0f172a;">{enc(m.FullName)}</td>
                          </tr>

                          <!-- COMPANY -->
                          <tr>
                            <td style="padding-bottom:18px;vertical-align:top;font-size:13px;font-weight:600;color:#64748b;">{labels.LabelCompany}:</td>
                            <td style="padding-bottom:18px;vertical-align:top;font-size:14px;color:#334155;">{enc(m.Company)}</td>
                          </tr>

                          <!-- EMAIL -->
                          <tr>
                            <td style="padding-bottom:18px;vertical-align:top;font-size:13px;font-weight:600;color:#64748b;">{labels.LabelEmail}:</td>
                            <td style="padding-bottom:18px;vertical-align:top;font-size:14px;"><a href="mailto:{enc(m.Email)}" style="color:#0284c7;font-weight:600;text-decoration:none;">{enc(m.Email)}</a></td>
                          </tr>

                          <!-- PHONE -->
                          <tr>
                            <td style="padding-bottom:18px;vertical-align:top;font-size:13px;font-weight:600;color:#64748b;">{labels.LabelPhone}:</td>
                            <td style="padding-bottom:18px;vertical-align:top;font-size:14px;font-weight:600;color:#0f172a;">{enc(m.Phone)}</td>
                          </tr>

                        </table>
                      </td>
                    </tr>

                    <!-- MESSAGE CONTAINER (QUOTED / CARD STYLE) -->
                    <tr>
                      <td style="padding:0 36px 32px;">
                        <div style="background-color:#f0f9ff;border-left:4px solid #0284c7;border-radius:0 8px 8px 0;padding:20px 24px;">
                          <div style="font-size:11px;font-weight:800;color:#0369a1;letter-spacing:1px;margin-bottom:10px;">{labels.LabelMessage}</div>
                          <div style="font-size:15px;line-height:1.65;color:#1e293b;font-weight:400;white-space:pre-wrap;">{messageHtml}</div>
                        </div>
                      </td>
                    </tr>

                    <!-- ACTION BUTTON -->
                    <tr>
                      <td style="padding:0 36px 36px;text-align:center;">
                        <a href="mailto:{enc(m.Email)}?subject=Re: SkyOS {enc(interestLabel)}" style="display:inline-block;padding:12px 28px;background-color:#0284c7;border-radius:6px;font-size:14px;font-weight:600;color:#ffffff;text-decoration:none;box-shadow:0 4px 12px rgba(2,132,199,0.25);">{labels.ReplyLabel} &rarr;</a>
                      </td>
                    </tr>

                    <!-- FOOTER METADATA -->
                    <tr>
                      <td style="background-color:#f8fafc;padding:16px 36px;border-top:1px solid #e2e8f0;font-size:12px;color:#94a3b8;text-align:center;">
                        <p style="margin:0 0 4px;">{labels.FooterNote}</p>
                        <p style="margin:0;font-size:11px;">{labels.MessageIdLabel}: #{enc(m.Id.ToString())} &middot; {labels.IpLabel}: {enc(m.IpAddress)}</p>
                      </td>
                    </tr>

                  </table>

                </td></tr>
              </table>
            </body>
            </html>
            """;
    }

    private static string BuildNotificationText(ContactMessage m, ContactEmailCopy.Labels labels, string interestLabel)
    {
        return $"""
         {labels.PlainHeading} #{m.Id}
         ─────────────────────────────────────────
         {labels.LabelName,-14}: {m.FullName}
         {labels.LabelCompany,-14}: {m.Company ?? "-"}
         {labels.LabelEmail,-14}: {m.Email}
         {labels.LabelPhone,-14}: {m.Phone ?? "-"}
         {labels.LabelTopic,-14}: {interestLabel}

         {labels.LabelMessage}:
         {m.Message}
         ─────────────────────────────────────────
         {labels.DateLabel}: {m.CreatedAtUtc:dd.MM.yyyy HH:mm} UTC | {labels.IpLabel}: {m.IpAddress}
         """;
    }
}
