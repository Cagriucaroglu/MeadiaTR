using MediatR;
using MediaTR.ApiService.Extensions;
using MediaTR.Application.Features.Categories.Queries.GetChildCategories;
using MediaTR.SharedKernel.Localization;
using MediaTR.SharedKernel.ResultAndError;
using Microsoft.AspNetCore.Mvc;

namespace MediaTR.ApiService.Endpoints.Categories;

/// <summary>
/// Get child categories endpoint
/// </summary>
internal sealed class GetChildCategories : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/categories/{parentId:guid}/children", async (
            [FromRoute] Guid parentId,
            ISender sender,
            ILocalizationService localizationService,
            CancellationToken cancellationToken) =>
        {
            GetChildCategoriesQuery query = new(parentId);

            Result<ChildCategoriesResult> result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.ToResponse(localizationService);
        })
        .WithName("GetChildCategories")
        .WithSummary("Get child categories of a parent category")
        .WithDescription(@"
Get direct children of a specific parent category.
- Cached for 30 minutes
- Returns only active categories
- Sorted by SortOrder
- Does NOT include nested children (only direct children)
- For full tree use /api/categories/tree
- Example: /api/categories/123e4567-e89b-12d3-a456-426614174000/children
        ")
        .WithOpenApi()
        .Produces<ChildCategoriesResult>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
}
