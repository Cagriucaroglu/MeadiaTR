using MediatR;
using MediaTR.ApiService.Endpoints;
using MediaTR.ApiService.Extensions;
using MediaTR.Application.Features.Orders.Commands;
using MediaTR.Domain.Enums;
using MediaTR.SharedKernel.ResultAndError;
using Microsoft.AspNetCore.Mvc;

namespace MediaTR.ApiService.Endpoints.Orders;

/// <summary>
/// Update order status endpoint
/// </summary>
internal sealed class UpdateOrderStatus : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/orders/{orderId:guid}/status", static async (
            Guid orderId,
            [FromBody] UpdateOrderStatusRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Guid correlationId = request.CorrelationId != Guid.Empty
                ? request.CorrelationId
                : Guid.NewGuid();

            // Create DTO
            UpdateOrderStatusDto dto = new(
                orderId,
                request.NewStatus,
                request.Notes,
                request.TrackingNumber
            );

            // Create command
            UpdateOrderStatusCommand command = new(dto, correlationId);

            // Execute command
            Result<Guid> result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.ToResponse();
        })
        .WithName("UpdateOrderStatus")
        .WithSummary("Update order status")
        .WithDescription("Updates the status of an existing order (Confirmed, Processing, Shipped, Delivered, Cancelled)")
        .WithOpenApi()
        .Produces<Guid>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
        // .RequireAuthorization(); // TODO: Add when auth is implemented
    }
}

/// <summary>
/// Update order status request DTO for API
/// </summary>
public record UpdateOrderStatusRequest(
    OrderStatus NewStatus,
    Guid CorrelationId = default,
    string? Notes = null,
    string? TrackingNumber = null);
