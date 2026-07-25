using SkyOS.Application.DTOs.Contact;
using SkyOS.Shared.Results;

namespace SkyOS.Application.Interfaces.Services;

/// <summary>
/// Owns the contact-message business rules: anti-bot screening, per-IP throttling,
/// persistence and team notification. Single responsibility — e-mail transport is delegated.
/// </summary>
public interface IContactMessageService
{
    Task<Result<ContactMessageResponseDto>> SubmitAsync(
        ContactMessageRequestDto request,
        CancellationToken cancellationToken = default);
}
