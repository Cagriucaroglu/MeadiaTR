using MediaTR.Application.Abstractions.Messaging;
using MediaTR.Application.BusinessLogic;
using MediaTR.Domain.Entities;
using MediaTR.SharedKernel.ResultAndError;

namespace MediaTR.Application.Features.Advertisements.Commands;

public class ApproveAdvertisementCommandHandler : ICommandHandler<ApproveAdvertisementCommand, ApproveAdvertisementResult>
{
    private readonly AdvertisementBusinessLogic _advertisementBusinessLogic;

    public ApproveAdvertisementCommandHandler(AdvertisementBusinessLogic advertisementBusinessLogic)
    {
        _advertisementBusinessLogic = advertisementBusinessLogic;
    }

    public async Task<Result<ApproveAdvertisementResult>> Handle(ApproveAdvertisementCommand request, CancellationToken cancellationToken)
    {
        // TODO: Get advertisement from repository
        // var advertisement = await _advertisementRepository.GetByIdAsync(request.AdvertisementId, cancellationToken);
        // if (advertisement == null)
        //     throw new NotFoundException("Advertisement not found");

        // For now, create a mock advertisement for demonstration
        Advertisement? advertisement = new()
        {
            Id = request.AdvertisementId,
            Status = MediaTR.Domain.Enums.AdvertisementStatus.PendingApproval
        };

        // Delegate to business logic - this will handle business rules and raise domain events
        _advertisementBusinessLogic.Approve(advertisement);

        // TODO: Save changes to repository
        // await _advertisementRepository.UpdateAsync(advertisement, cancellationToken);
        // await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Return result
        return Result.Success(new ApproveAdvertisementResult(
            advertisement.Id,
            advertisement.Status.ToString(),
            advertisement.PublishedAt,
            advertisement.ExpiresAt
        ));
    }
}