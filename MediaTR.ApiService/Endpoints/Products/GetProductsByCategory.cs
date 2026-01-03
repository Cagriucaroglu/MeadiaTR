using MediatR;
using MediaTR.ApiService.Endpoints;
using MediaTR.ApiService.Extensions;
using MediaTR.Application.Features.Products.Queries.GetProductsByCategory;
using MediaTR.SharedKernel.Localization;
using MediaTR.SharedKernel.ResultAndError;
using Microsoft.AspNetCore.Mvc;

namespace MediaTR.ApiService.Endpoints.Products;

/// <summary>
/// Get products by category endpoint
/// </summary>
internal sealed class GetProductsByCategory : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/products/category/{categoryId:guid}", async (
            Guid categoryId,
            ISender sender,
            ILocalizationService localizationService,
            CancellationToken cancellationToken) =>
        {
            GetProductsByCategoryQuery query = new(categoryId);

            Result<GetProductsByCategoryResult> result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.ToResponse(localizationService);
        })
        .WithName("GetProductsByCategory")
        .WithSummary("Get products by category")
        .WithDescription("Retrieves all products in a specific category (cached for 30 minutes)")
        .WithOpenApi()
        .Produces<GetProductsByCategoryResult>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
}
