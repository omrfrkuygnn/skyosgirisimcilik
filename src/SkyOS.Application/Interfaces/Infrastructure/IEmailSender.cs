using SkyOS.Application.DTOs.Common;
using SkyOS.Shared.Results;

namespace SkyOS.Application.Interfaces.Infrastructure;

/// <summary>
/// Abstraction over the outbound e-mail transport. Implemented with MailKit/SMTP in
/// Infrastructure. Returns a <see cref="Result"/> instead of throwing so callers decide policy.
/// </summary>
public interface IEmailSender
{
    Task<Result> SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
