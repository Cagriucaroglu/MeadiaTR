using MediatR;
using MediaTR.ApiService.Endpoints;
using MediaTR.ApiService.Extensions;
using MediaTR.Application.Features.Categories.Commands;
using MediaTR.SharedKernel.Localization;
using MediaTR.SharedKernel.ResultAndError;
using Microsoft.AspNetCore.Mvc;

namespace MediaTR.ApiService.Endpoints.Categories;

/// <summary>
/// Create category endpoint
/// </summary>
internal sealed class CreateCategory : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/categories", async (
            [FromBody] CreateCategoryRequest request,
            ISender sender,
            ILocalizationService localizationService,
            CancellationToken cancellationToken) =>
        {
            Guid correlationId = request.CorrelationId != Guid.Empty
                ? request.CorrelationId
                : Guid.NewGuid();

            // Create DTO
            CreateCategoryDto dto = new(
                request.Name,
                request.Description,
                request.ParentCategoryId,
                request.SortOrder
            );

            // Create command
            CreateCategoryCommand command = new(dto, correlationId);

            // Execute command
            Result<Guid> result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.ToResponse(localizationService);
        })
        .WithName("CreateCategory")
        .WithSummary("Create a new category")
        .WithDescription("Creates a new product category with optional parent category for hierarchy")
        .WithOpenApi()
        .Produces<Guid>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
        // .RequireAuthorization(); // TODO: Add when auth is implemented
    }
}

/// <summary>
/// Create category request DTO for API
/// </summary>
public record CreateCategoryRequest(
    string Name,
    string Description,
    Guid CorrelationId = default,
    Guid? ParentCategoryId = null,
    int SortOrder = 0);
