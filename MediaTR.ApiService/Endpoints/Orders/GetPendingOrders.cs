using MediatR;
using MediaTR.ApiService.Endpoints;
using MediaTR.ApiService.Extensions;
using MediaTR.Application.Features.Orders.DTOs;
using MediaTR.Application.Features.Orders.Queries;
using MediaTR.SharedKernel.Localization;
using MediaTR.SharedKernel.ResultAndError;
using Microsoft.AspNetCore.Mvc;

namespace MediaTR.ApiService.Endpoints.Orders;

/// <summary>
/// Get all pending orders endpoint
/// </summary>
internal sealed class GetPendingOrders : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/orders/pending", async (
            ISender sender,
            ILocalizationService localizationService,
            CancellationToken cancellationToken) =>
        {
            GetPendingOrdersQuery query = new();

            Result<List<GetOrderResult>> result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.ToResponse(localizationService);
        })
        .WithName("GetPendingOrders")
        .WithSummary("Get all pending orders")
        .WithDescription("Retrieves all orders with Pending status")
        .WithOpenApi()
        .Produces<List<GetOrderResult>>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
        // .RequireAuthorization(); // TODO: Add when auth is implemented - admin only
    }
}
