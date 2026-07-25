using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using SkyOS.Application.DTOs.Common;
using SkyOS.Application.Interfaces.Infrastructure;
using SkyOS.Application.Options;
using SkyOS.Infrastructure.Options;
using SkyOS.Shared.Results;

namespace SkyOS.Infrastructure.Services;

/// <summary>
/// MailKit/SMTP implementation of <see cref="IEmailSender"/>. Recipient is the configured
/// team inbox (see ContactForm options in the calling service). Returns a Result so callers
/// decide whether a transport failure should surface to the user.
/// </summary>
public sealed class EmailSender : IEmailSender
{
    private readonly SmtpOptions _options;
    private readonly ContactFormOptions _contactFormOptions;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(
        IOptions<SmtpOptions> options,
        IOptions<ContactFormOptions> contactFormOptions,
        ILogger<EmailSender> logger)
    {
        _options = options.Value;
        _contactFormOptions = contactFormOptions.Value;
        _logger = logger;
    }

    public async Task<Result> SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.Host) || string.IsNullOrWhiteSpace(_options.FromAddress))
        {
            _logger.LogWarning("SMTP is not configured; e-mail was not sent.");
            return Result.Failure(Error.Failure("E-posta gönderimi yapılandırılmamış."));
        }

        var recipientEmail = _contactFormOptions.NotificationRecipientEmail;
        var recipientName = _contactFormOptions.NotificationRecipientName;
        if (string.IsNullOrWhiteSpace(recipientEmail))
        {
            return Result.Failure(Error.Failure("Bildirim alıcısı yapılandırılmamış."));
        }

        var mime = new MimeMessage();
        mime.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
        mime.To.Add(new MailboxAddress(recipientName, recipientEmail));
        mime.Subject = message.Subject;

        if (!string.IsNullOrWhiteSpace(message.ReplyToAddress))
        {
            mime.ReplyTo.Add(new MailboxAddress(message.ReplyToName ?? string.Empty, message.ReplyToAddress));
        }

        mime.Body = new BodyBuilder
        {
            HtmlBody = message.HtmlBody,
            TextBody = message.PlainTextBody ?? string.Empty,
        }.ToMessageBody();

        try
        {
            using var client = new SmtpClient();
            var socketOptions = _options.UseStartTls
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.SslOnConnect;

            await client.ConnectAsync(_options.Host, _options.Port, socketOptions, cancellationToken).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(_options.Username))
            {
                await client.AuthenticateAsync(_options.Username, _options.Password, cancellationToken).ConfigureAwait(false);
            }

            await client.SendAsync(mime, cancellationToken).ConfigureAwait(false);
            await client.DisconnectAsync(true, cancellationToken).ConfigureAwait(false);

            return Result.Success();
        }
        catch (Exception ex)
        {
            // Never log message bodies/PII; just the failure reason.
            _logger.LogError(ex, "SMTP send failed to host {Host}:{Port}.", _options.Host, _options.Port);
            return Result.Failure(Error.Failure("E-posta gönderilirken bir hata oluştu."));
        }
    }
}
