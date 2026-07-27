using Mapster;
using SkyOS.Application.DTOs.Admin;
using SkyOS.Application.Interfaces.Persistence;
using SkyOS.Application.Interfaces.Services;
using SkyOS.Domain.Entities;
using SkyOS.Shared.Results;

namespace SkyOS.Application.Services;

public sealed class ContactMessageAdminService : IContactMessageAdminService
{
    private readonly IUnitOfWork _unitOfWork;

    public ContactMessageAdminService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<IReadOnlyList<ContactMessageListItemDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.Repository<ContactMessage>().ListAsync(cancellationToken).ConfigureAwait(false);
        return items
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new ContactMessageListItemDto
            {
                Id = x.Id,
                FullName = x.FullName,
                Email = x.Email,
                InterestType = x.InterestType,
                MessagePreview = Truncate(x.Message, 120),
                IsRead = x.IsRead,
                CreatedAtUtc = x.CreatedAtUtc,
            })
            .ToList();
    }

    public async Task<Result<ContactMessageDetailDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Repository<ContactMessage>().GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        return entity is null
            ? Result.Failure<ContactMessageDetailDto>(Error.NotFound("Contact message not found."))
            : Result.Success(entity.Adapt<ContactMessageDetailDto>());
    }

    public async Task<Result> MarkAsReadAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Repository<ContactMessage>().GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (entity is null)
        {
            return Result.Failure(Error.NotFound("Contact message not found."));
        }

        entity.IsRead = true;
        _unitOfWork.Repository<ContactMessage>().Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Repository<ContactMessage>().GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (entity is null)
        {
            return Result.Failure(Error.NotFound("Contact message not found."));
        }

        _unitOfWork.Repository<ContactMessage>().Remove(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..max] + "…";
}
