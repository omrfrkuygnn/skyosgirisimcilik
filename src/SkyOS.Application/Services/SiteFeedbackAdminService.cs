using Mapster;
using SkyOS.Application.DTOs.Admin;
using SkyOS.Application.Interfaces.Persistence;
using SkyOS.Application.Interfaces.Services;
using SkyOS.Domain.Entities;
using SkyOS.Shared.Results;

namespace SkyOS.Application.Services;

public sealed class SiteFeedbackAdminService : ISiteFeedbackAdminService
{
    private readonly IUnitOfWork _unitOfWork;

    public SiteFeedbackAdminService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<IReadOnlyList<SiteFeedbackListItemDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.Repository<SiteFeedback>().ListAsync(cancellationToken).ConfigureAwait(false);
        return items
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new SiteFeedbackListItemDto
            {
                Id = x.Id,
                FullName = x.FullName,
                Email = x.Email,
                Category = x.Category,
                MessagePreview = Truncate(x.Message, 120),
                IsRead = x.IsRead,
                Culture = x.Culture,
                CreatedAtUtc = x.CreatedAtUtc,
            })
            .ToList();
    }

    public async Task<Result<SiteFeedbackDetailDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Repository<SiteFeedback>().GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        return entity is null
            ? Result.Failure<SiteFeedbackDetailDto>(Error.NotFound("Feedback not found."))
            : Result.Success(entity.Adapt<SiteFeedbackDetailDto>());
    }

    public async Task<Result> MarkAsReadAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Repository<SiteFeedback>().GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (entity is null)
        {
            return Result.Failure(Error.NotFound("Feedback not found."));
        }

        entity.IsRead = true;
        _unitOfWork.Repository<SiteFeedback>().Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Repository<SiteFeedback>().GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (entity is null)
        {
            return Result.Failure(Error.NotFound("Feedback not found."));
        }

        _unitOfWork.Repository<SiteFeedback>().Remove(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..max] + "…";
}
