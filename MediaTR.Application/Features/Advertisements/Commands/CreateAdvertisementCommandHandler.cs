using MediaTR.Application.Abstractions.Messaging;
using MediaTR.Application.BusinessLogic;
using MediaTR.Application.Features.Advertisements.DTOs;
using MediaTR.SharedKernel.ResultAndError;

namespace MediaTR.Application.Features.Advertisements.Commands;

public class CreateAdvertisementCommandHandler : ICommandHandler<CreateAdvertisementCommand, CreateAdvertisementResult>
{
    private readonly AdvertisementBusinessLogic _advertisementBusinessLogic;

    public CreateAdvertisementCommandHandler(AdvertisementBusinessLogic advertisementBusinessLogic)
    {
        _advertisementBusinessLogic = advertisementBusinessLogic;
    }

    public async Task<Result<CreateAdvertisementResult>> Handle(CreateAdvertisementCommand request, CancellationToken cancellationToken)
    {
        // Delegate to business logic
        var advertisement = _advertisementBusinessLogic.CreateAdvertisement(
            request.Title,
            request.Description,
            request.ProductId,
            request.SellerId,
            request.Price,
            request.IsNegotiable,
            request.IsUrgent,
            request.ContactPhone,
            request.ContactEmail
        );

        // TODO: Save to repository
        // await _advertisementRepository.AddAsync(advertisement, cancellationToken);

        // Return result
        return Result.Success(new CreateAdvertisementResult(
            advertisement.Id,
            advertisement.Title,
            advertisement.ProductId,
            advertisement.SellerId,
            advertisement.Price,
            advertisement.Status.ToString(),
            advertisement.IsNegotiable,
            advertisement.IsUrgent
        ));
    }
}