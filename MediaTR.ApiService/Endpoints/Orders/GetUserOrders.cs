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
/// Get all orders for a specific user endpoint
/// </summary>
internal sealed class GetUserOrders : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/orders/user/{userId:guid}", async (
            Guid userId,
            ISender sender,
            ILocalizationService localizationService,
            CancellationToken cancellationToken) =>
        {
            GetUserOrdersQuery query = new(userId);

            Result<List<GetOrderResult>> result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.ToResponse(localizationService);
        })
        .WithName("GetUserOrders")
        .WithSummary("Get all orders for a user")
        .WithDescription("Retrieves all orders placed by a specific user")
        .WithOpenApi()
        .Produces<List<GetOrderResult>>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
        // .RequireAuthorization(); // TODO: Add when auth is implemented
    }
}
