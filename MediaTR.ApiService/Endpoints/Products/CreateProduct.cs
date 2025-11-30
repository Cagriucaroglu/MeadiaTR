using MediatR;
using MediaTR.ApiService.Endpoints;
using MediaTR.ApiService.Extensions;
using MediaTR.Application.Features.Products.Commands;
using MediaTR.Domain.ValueObjects;
using MediaTR.SharedKernel.Localization;
using MediaTR.SharedKernel.ResultAndError;
using Microsoft.AspNetCore.Mvc;

namespace MediaTR.ApiService.Endpoints.Products;

/// <summary>
/// Create product endpoint with outbox pattern for domain events
/// </summary>
internal sealed class CreateProduct : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/products", async (
            [FromBody] CreateProductRequest request,
            ISender sender,
            ILocalizationService localizationService,
            CancellationToken cancellationToken) =>
        {
            Guid correlationId = request.CorrelationId != Guid.Empty
                ? request.CorrelationId
                : Guid.NewGuid();

            // Create DTO
            CreateProductDto dto = new(
                request.Name,
                request.Description,
                request.CategoryId,
                request.Price,
                request.Sku,
                request.StockQuantity,
                request.Weight
            );

            // Create command
            CreateProductCommand command = new(dto, correlationId);

            // Execute command
            Result<Guid> result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.ToResponse(localizationService);
        })
        .WithName("CreateProduct")
        .WithSummary("Create a new product")
        .WithDescription("Creates a new product and publishes ProductCreated domain event")
        .WithOpenApi()
        .Produces<Guid>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
        // .RequireAuthorization(); // TODO: Add when auth is implemented
    }
}

/// <summary>
/// Create product request DTO for API
/// </summary>
public record CreateProductRequest(
    string Name,
    string Description,
    Guid CategoryId,
    Money Price,
    string Sku,
    int StockQuantity,
    Guid CorrelationId = default,
    double Weight = 0);
