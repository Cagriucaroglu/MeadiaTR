using MediaTR.Application.Abstractions.Messaging;
using MediaTR.Application.BusinessLogic;
using MediaTR.Domain.Entities;
using MediaTR.Domain.Repositories;
using MediaTR.SharedKernel.BusinessLogic;
using MediaTR.SharedKernel.ResultAndError;

namespace MediaTR.Application.Features.Advertisements.Commands;

/// <summary>
/// CreateAdvertisementCommand handler with transactional support
/// Note: No outbox event needed for creation - only for approval/publish
/// </summary>
internal sealed class CreateAdvertisementCommandHandler : TransactionalCommandHandlerBase<CreateAdvertisementCommand, Guid>
{
    private readonly IAdvertisementRepository _advertisementRepository;
    private readonly AdvertisementBusinessLogic _advertisementBusinessLogic;

    public CreateAdvertisementCommandHandler(
        IServiceProvider serviceProvider,
        IAdvertisementRepository advertisementRepository,
        AdvertisementBusinessLogic advertisementBusinessLogic)
        : base(serviceProvider)
    {
        _advertisementRepository = advertisementRepository;
        _advertisementBusinessLogic = advertisementBusinessLogic;
    }

    protected override async Task<Result<Guid>> ProcessCommandAsync(
        CreateAdvertisementCommand request,
        CancellationToken cancellationToken)
    {
        // Business logic ile advertisement oluştur
        Advertisement advertisement = _advertisementBusinessLogic.CreateAdvertisement(
            request.Request.Title,
            request.Request.Description,
            request.Request.ProductId,
            request.Request.SellerId,
            request.Request.Price,
            request.CorrelationId,
            request.Request.IsNegotiable,
            request.Request.IsUrgent,
            request.Request.ContactPhone,
            request.Request.ContactEmail
        );

        // Repository'ye kaydet
        await _advertisementRepository.AddAsync(advertisement);

        return Result.Success(advertisement.Id);
    }
}