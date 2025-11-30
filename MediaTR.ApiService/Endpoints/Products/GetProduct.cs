using MediatR;
using MediaTR.ApiService.Endpoints;
using MediaTR.ApiService.Extensions;
using MediaTR.Application.Features.Products.Queries;
using MediaTR.SharedKernel.Localization;
using MediaTR.SharedKernel.ResultAndError;
using Microsoft.AspNetCore.Mvc;

namespace MediaTR.ApiService.Endpoints.Products;

/// <summary>
/// Get product by ID endpoint
/// </summary>
internal sealed class GetProduct : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/products/{productId:guid}", async (
            Guid productId,
            ISender sender,
            ILocalizationService localizationService,
            CancellationToken cancellationToken) =>
        {
            GetProductQuery query = new(productId);

            Result<GetProductResult> result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.ToResponse(localizationService);
        })
        .WithName("GetProduct")
        .WithSummary("Get product by ID")
        .WithDescription("Retrieves a specific product by its unique identifier")
        .WithOpenApi()
        .Produces<GetProductResult>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
}
