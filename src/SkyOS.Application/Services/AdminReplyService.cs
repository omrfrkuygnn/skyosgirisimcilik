using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SkyOS.Application.DTOs.Admin;
using SkyOS.Application.DTOs.Common;
using SkyOS.Application.Interfaces.Infrastructure;
using SkyOS.Application.Interfaces.Persistence;
using SkyOS.Application.Interfaces.Services;
using SkyOS.Application.Options;
using SkyOS.Domain.Entities;
using SkyOS.Shared.Results;

namespace SkyOS.Application.Services;

public sealed class AdminReplyService : IAdminReplyService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailSender _emailSender;
    private readonly ContactFormOptions _contactFormOptions;
    private readonly ILogger<AdminReplyService> _logger;

    public AdminReplyService(
        IUnitOfWork unitOfWork,
        IEmailSender emailSender,
        IOptions<ContactFormOptions> contactFormOptions,
        ILogger<AdminReplyService> logger)
    {
        _unitOfWork = unitOfWork;
        _emailSender = emailSender;
        _contactFormOptions = contactFormOptions.Value;
        _logger = logger;
    }

    public async Task<Result> ReplyToContactAsync(
        int contactMessageId,
        AdminReplyDto reply,
        string adminEmail,
        string? adminDisplayName,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Repository<ContactMessage>()
            .GetByIdAsync(contactMessageId, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return Result.Failure(Error.NotFound("Contact message not found."));
        }

        if (string.IsNullOrWhiteSpace(entity.Email))
        {
            return Result.Failure(Error.Validation("Bu kayıtta e-posta adresi bulunmuyor."));
        }

        var labels = AdminReplyEmailCopy.ForCulture("tr");
        var email = BuildEmail(entity.FullName, entity.Email, entity.Message, reply, labels, adminEmail, adminDisplayName);
        var send = await _emailSender.SendAsync(email, cancellationToken).ConfigureAwait(false);
        if (send.IsFailure)
        {
            return send;
        }

        entity.IsRead = true;
        _unitOfWork.Repository<ContactMessage>().Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Admin reply sent for contact message #{Id} to {Email}.", entity.Id, entity.Email);
        return Result.Success();
    }

    public async Task<Result> ReplyToFeedbackAsync(
        int feedbackId,
        AdminReplyDto reply,
        string adminEmail,
        string? adminDisplayName,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Repository<SiteFeedback>()
            .GetByIdAsync(feedbackId, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return Result.Failure(Error.NotFound("Feedback not found."));
        }

        if (string.IsNullOrWhiteSpace(entity.Email))
        {
            return Result.Failure(Error.Validation("Bu kayıtta e-posta adresi bulunmuyor."));
        }

        var labels = AdminReplyEmailCopy.ForCulture(entity.Culture);
        var email = BuildEmail(entity.FullName, entity.Email, entity.Message, reply, labels, adminEmail, adminDisplayName);
        var send = await _emailSender.SendAsync(email, cancellationToken).ConfigureAwait(false);
        if (send.IsFailure)
        {
            return send;
        }

        entity.IsRead = true;
        _unitOfWork.Repository<SiteFeedback>().Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Admin reply sent for feedback #{Id} to {Email}.", entity.Id, entity.Email);
        return Result.Success();
    }

    private EmailMessage BuildEmail(
        string recipientName,
        string recipientEmail,
        string originalMessage,
        AdminReplyDto reply,
        AdminReplyEmailCopy.Labels labels,
        string adminEmail,
        string? adminDisplayName)
    {
        static string enc(string? value) => System.Net.WebUtility.HtmlEncode(value ?? string.Empty);
        var bodyHtml = enc(reply.Message).Replace("\n", "<br />", StringComparison.Ordinal);
        var originalHtml = enc(originalMessage).Replace("\n", "<br />", StringComparison.Ordinal);
        var fromName = _contactFormOptions.NotificationRecipientName;
        var replyToName = string.IsNullOrWhiteSpace(adminDisplayName) ? fromName : adminDisplayName;
        var replyToAddress = ResolveReplyToAddress(adminEmail);

        var html = $"""
            <!DOCTYPE html>
            <html lang="{labels.Lang}">
            <head><meta charset="UTF-8" /><title>{enc(reply.Subject)}</title></head>
            <body style="margin:0;padding:0;background:#f4f6f9;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">
              <table width="100%" cellpadding="0" cellspacing="0" style="background:#f4f6f9;padding:32px 16px;">
                <tr><td align="center">
                  <table width="620" cellpadding="0" cellspacing="0" style="max-width:620px;width:100%;background:#fff;border-radius:12px;border:1px solid #e2e8f0;overflow:hidden;">
                    <tr>
                      <td style="background:#0f172a;padding:24px 32px;">
                        <span style="font-size:22px;font-weight:800;color:#fff;">Sky<span style="color:#38bdf8;">OS</span></span>
                        <span style="display:block;margin-top:8px;font-size:13px;color:#94a3b8;">{enc(labels.Heading)}</span>
                      </td>
                    </tr>
                    <tr>
                      <td style="padding:28px 32px 12px;font-size:15px;line-height:1.7;color:#1e293b;">
                        <p style="margin:0 0 16px;">{enc(labels.Greeting)} {enc(recipientName)},</p>
                        <div>{bodyHtml}</div>
                      </td>
                    </tr>
                    <tr>
                      <td style="padding:0 32px 28px;">
                        <div style="background:#f8fafc;border-left:4px solid #cbd5e1;border-radius:0 8px 8px 0;padding:16px 18px;">
                          <div style="font-size:11px;font-weight:700;color:#64748b;letter-spacing:.08em;margin-bottom:8px;">{enc(labels.OriginalHeading)}</div>
                          <div style="font-size:14px;line-height:1.6;color:#475569;">{originalHtml}</div>
                        </div>
                      </td>
                    </tr>
                    <tr>
                      <td style="padding:0 32px 28px;font-size:13px;color:#64748b;border-top:1px solid #e2e8f0;">
                        <p style="margin:16px 0 0;">{enc(labels.Footer)}</p>
                      </td>
                    </tr>
                  </table>
                </td></tr>
              </table>
            </body>
            </html>
            """;

        var plain = $"""
            {labels.Greeting} {recipientName},

            {reply.Message}

            ---
            {labels.OriginalHeading}
            {originalMessage}

            {labels.Footer}
            """;

        return new EmailMessage
        {
            ToEmail = recipientEmail,
            ToName = recipientName,
            Subject = reply.Subject,
            HtmlBody = html,
            PlainTextBody = plain,
            ReplyToAddress = replyToAddress,
            ReplyToName = replyToName,
        };
    }

    private string ResolveReplyToAddress(string adminEmail)
    {
        if (!string.IsNullOrWhiteSpace(adminEmail)
            && adminEmail.Contains('@', StringComparison.Ordinal)
            && !adminEmail.EndsWith(".local", StringComparison.OrdinalIgnoreCase))
        {
            return adminEmail;
        }

        return _contactFormOptions.NotificationRecipientEmail;
    }
}
