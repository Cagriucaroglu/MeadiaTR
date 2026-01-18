using MediatR;
using MediaTR.ApiService.Extensions;
using MediaTR.Application.Features.Orders.Commands.CreateOrder;
using MediaTR.SharedKernel.Localization;
using MediaTR.SharedKernel.ResultAndError;
using Microsoft.AspNetCore.Mvc;

namespace MediaTR.ApiService.Endpoints.Orders;

/// <summary>
/// Create order endpoint (Checkout)
/// Converts shopping cart to order
/// </summary>
internal sealed class CreateOrder : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/orders", async (
            [FromBody] CreateOrderRequest request,
            ISender sender,
            ILocalizationService localizationService,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateOrderCommand(
                request.ShippingStreet,
                request.ShippingCity,
                request.ShippingState ?? "",
                request.ShippingPostalCode,
                request.ShippingCountry,
                request.BillingStreet,
                request.BillingCity,
                request.BillingState,
                request.BillingPostalCode,
                request.BillingCountry,
                request.PaymentMethod,
                request.Notes);

            Result<CreateOrderResult> result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.ToResponse(localizationService);
        })
        .WithName("CreateOrder")
        .WithSummary("Create order from shopping cart (Checkout)")
        .WithDescription(@"
Create order from current shopping cart:
- Validates stock availability
- Deducts inventory
- Calculates shipping & tax
- Clears cart after successful order
- Requires authentication
- Example: POST /api/orders
        ")
        .WithOpenApi()
        .Produces<CreateOrderResult>(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
        .RequireAuthorization();
    }
}

public record CreateOrderRequest(
    string ShippingStreet,
    string ShippingCity,
    string? ShippingState,
    string ShippingPostalCode,
    string ShippingCountry,
    string? BillingStreet,
    string? BillingCity,
    string? BillingState,
    string? BillingPostalCode,
    string? BillingCountry,
    string PaymentMethod,
    string? Notes);
