using MediatR;
using MediaTR.Application.BusinessLogic;
using MediaTR.Domain.Repositories;
using MediaTR.SharedKernel.ResultAndError;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace MediaTR.Application.Features.Orders.Commands.CancelOrder;

/// <summary>
/// CancelOrder command handler
/// Validates ownership and order status before cancelling
/// </summary>
internal sealed class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, Result<bool>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly OrderBusinessLogic _orderBusinessLogic;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CancelOrderCommandHandler(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        OrderBusinessLogic orderBusinessLogic,
        IHttpContextAccessor httpContextAccessor)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _orderBusinessLogic = orderBusinessLogic;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<bool>> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        // Get authenticated user
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return Error.Failure("Http.ContextNotAvailable", "HTTP context is not available");

        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Error.Unauthorized("Auth.Unauthorized", "User must be authenticated");

        // Get order
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
            return Error.NotFound("Order.NotFound", $"Order {request.OrderId} not found");

        // Verify ownership
        if (order.UserId != userId)
            return Error.Forbidden("Order.NotOwner", "You can only cancel your own orders");

        // Validate can be cancelled
        if (!order.CanBeCancelled)
            return Error.Validation("Order.CannotCancel", $"Order cannot be cancelled in {order.Status} status");

        // Cancel order using BusinessLogic
        _orderBusinessLogic.CancelOrder(order, request.CorrelationId);

        // Restore stock for all items
        await RestoreStockAsync(order, cancellationToken);

        // Update order in MongoDB
        await _orderRepository.UpdateAsync(order, request.CorrelationId, cancellationToken: cancellationToken);

        return true;
    }

    private async Task RestoreStockAsync(Domain.Entities.Order order, CancellationToken cancellationToken)
    {
        foreach (var orderItem in order.OrderItems)
        {
            var product = await _productRepository.GetByIdAsync(orderItem.ProductId, cancellationToken);
            if (product == null)
                continue;

            // Restore stock
            product.StockQuantity += orderItem.Quantity;

            // Update product in MongoDB
            await _productRepository.UpdateAsync(product, cancellationToken: cancellationToken);
        }
    }
}
