using MediatR;
using MediaTR.ApiService.Endpoints;
using MediaTR.ApiService.Extensions;
using MediaTR.Application.Features.Orders.DTOs;
using MediaTR.Application.Features.Orders.Queries;
using MediaTR.SharedKernel.ResultAndError;
using Microsoft.AspNetCore.Mvc;

namespace MediaTR.ApiService.Endpoints.Orders;

/// <summary>
/// Get order by ID endpoint
/// </summary>
internal sealed class GetOrder : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/orders/{orderId:guid}", static async (
            Guid orderId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            GetOrderQuery query = new(orderId);

            Result<GetOrderResult> result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.ToResponse();
        })
        .WithName("GetOrder")
        .WithSummary("Get order by ID")
        .WithDescription("Retrieves a specific order by its unique identifier")
        .WithOpenApi()
        .Produces<GetOrderResult>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
        // .RequireAuthorization(); // TODO: Add when auth is implemented
    }
}
