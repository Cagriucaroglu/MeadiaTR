using MediaTR.Application.Abstractions.Messaging;
using MediaTR.Application.BusinessLogic;
using MediaTR.Domain.Entities;
using MediaTR.Domain.Events.Entities;
using MediaTR.Domain.Events;
using MediaTR.Domain.Repositories;
using MediaTR.SharedKernel.BusinessLogic;
using MediaTR.SharedKernel.Outbox;
using MediaTR.SharedKernel.ResultAndError;
using System.Text.Json;

namespace MediaTR.Application.Features.Products.Commands;

/// <summary>
/// CreateProductCommand handler with transactional support and fire-and-wait outbox pattern
/// </summary>
internal sealed class CreateProductCommandHandler : TransactionalCommandHandlerBase<CreateProductCommand, Guid>
{
    private readonly IProductRepository _productRepository;
    private readonly ProductBusinessLogic _productBusinessLogic;

    public CreateProductCommandHandler(
        IServiceProvider serviceProvider,
        IProductRepository productRepository,
        ProductBusinessLogic productBusinessLogic)
        : base(serviceProvider)
    {
        _productRepository = productRepository;
        _productBusinessLogic = productBusinessLogic;
    }

    protected override async Task<Result<Guid>> ProcessCommandAsync(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        // Business logic ile product oluştur
        Product product = _productBusinessLogic.CreateProduct(
            request.Request.Name,
            request.Request.Description,
            request.Request.CategoryId,
            request.Request.Price,
            request.Request.Sku,
            request.Request.StockQuantity,
            request.CorrelationId,
            request.Request.Weight
        );

        // Repository'ye kaydet
        await _productRepository.AddAsync(product);

        // Outbox Event oluştur (Fire-and-Wait Pattern)
        var productCreatedEvent = new ProductCreatedEvent
        {
            Payload = product,
            CorrelationId = request.CorrelationId
        };

        var outboxEvent = new OutboxEvent
        {
            Id = Guid.NewGuid(),
            EventType = "ProductCreated",
            AggregateId = product.Id,
            AggregateType = nameof(Product),
            Payload = JsonSerializer.Serialize(productCreatedEvent),
            Status = OutboxStatus.Immediate,
            CreatedAt = DateTimeOffset.UtcNow,
            CorrelationId = request.CorrelationId,
            ConsistencyLevel = ConsistencyLevel.Strong
        };

        // OutboxEvent'i BusinessLogicContext'e track et
        request.BusinessLogicContext.TrackOutboxEvent(outboxEvent);

        // OutboxEvent'i database'e ekle (transaction içinde)
        var outboxRepository = Context.GetRepository<OutboxEvent>();
        await outboxRepository.AddAsync(outboxEvent);

        return Result.Success(product.Id);
    }

    /// <summary>
    /// Transaction commit'ten ÖNCE event'leri işle (Fire-and-Wait)
    /// </summary>
    protected override async Task BeforeTransactionCommittedAsync(
        Result<Guid> result,
        BusinessLogicContext blContext,
        CancellationToken cancellationToken)
    {
        if (result.IsSuccess)
        {
            await ProcessOutboxEventsAsync(blContext, cancellationToken);
        }
    }
}