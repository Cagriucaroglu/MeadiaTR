using MediatR;
using MediaTR.ApiService.Extensions;
using MediaTR.Application.Features.Categories.Queries.GetCategoryBySlug;
using MediaTR.SharedKernel.Localization;
using MediaTR.SharedKernel.ResultAndError;
using Microsoft.AspNetCore.Mvc;

namespace MediaTR.ApiService.Endpoints.Categories;

/// <summary>
/// Get category by slug endpoint
/// </summary>
internal sealed class GetCategoryBySlug : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/categories/{slug}", async (
            [FromRoute] string slug,
            ISender sender,
            ILocalizationService localizationService,
            CancellationToken cancellationToken) =>
        {
            GetCategoryBySlugQuery query = new(slug);

            Result<CategoryDto?> result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.ToResponse(localizationService);
        })
        .WithName("GetCategoryBySlug")
        .WithSummary("Get category by slug")
        .WithDescription(@"
Get category details by slug (SEO-friendly URL identifier).
- Cached for 1 hour
- Returns category details with parent info
- Example: /api/categories/elektronik
        ")
        .WithOpenApi()
        .Produces<CategoryDto>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
}
