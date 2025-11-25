using MediatR;
using MediaTR.ApiService.Endpoints;
using MediaTR.ApiService.Extensions;
using MediaTR.Application.Features.Orders.Commands;
using MediaTR.Domain.ValueObjects;
using MediaTR.SharedKernel.ResultAndError;
using Microsoft.AspNetCore.Mvc;

namespace MediaTR.ApiService.Endpoints.Orders;

/// <summary>
/// Create order endpoint with fire-and-wait outbox pattern
/// </summary>
internal sealed class CreateOrder : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/orders", static async (
            [FromBody] CreateOrderRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Guid correlationId = request.CorrelationId != Guid.Empty
                ? request.CorrelationId
                : Guid.NewGuid();

            // Create DTO
            PlaceOrderDto dto = new(
                request.UserId,
                request.OrderItems.Select(item => new OrderItemRequest(
                    item.ProductId,
                    item.Quantity,
                    item.UnitPrice)).ToList(),
                request.ShippingAddress,
                request.BillingAddress,
                request.Notes,
                request.PaymentMethod
            );

            // Create command with CommandWrapper pattern
            PlaceOrderCommand command = new(dto, correlationId);

            // Execute command
            Result<Guid> result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            // Return result
            return result.ToResponse();
        })
        .WithName("CreateOrder")
        .WithSummary("Creates a new order")
        .WithDescription("Creates a new order with fire-and-wait outbox pattern for eventual consistency")
        .WithOpenApi()
        .Produces<Guid>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
        // .RequireAuthorization(); // TODO: Add when auth is implemented
    }
}

/// <summary>
/// Create order request DTO for API
/// </summary>
public record CreateOrderRequest(
    Guid UserId,
    List<CreateOrderItemRequest> OrderItems,
    Address ShippingAddress,
    Address BillingAddress,
    Guid CorrelationId = default,
    string? Notes = null,
    string? PaymentMethod = null);

/// <summary>
/// Order item request for API
/// </summary>
public record CreateOrderItemRequest(
    Guid ProductId,
    int Quantity,
    Money UnitPrice);
