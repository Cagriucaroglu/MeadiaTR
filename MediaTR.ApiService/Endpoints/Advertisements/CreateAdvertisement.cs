using MediatR;
using MediaTR.ApiService.Endpoints;
using MediaTR.ApiService.Extensions;
using MediaTR.Application.Features.Advertisements.Commands;
using MediaTR.Domain.ValueObjects;
using MediaTR.SharedKernel.Localization;
using MediaTR.SharedKernel.ResultAndError;
using Microsoft.AspNetCore.Mvc;

namespace MediaTR.ApiService.Endpoints.Advertisements;

/// <summary>
/// Create advertisement endpoint
/// </summary>
internal sealed class CreateAdvertisement : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/advertisements", async (
            [FromBody] CreateAdvertisementRequest request,
            ISender sender,
            ILocalizationService localizationService,
            CancellationToken cancellationToken) =>
        {
            Guid correlationId = request.CorrelationId != Guid.Empty
                ? request.CorrelationId
                : Guid.NewGuid();

            // Create DTO
            CreateAdvertisementDto dto = new(
                request.Title,
                request.Description,
                request.ProductId,
                request.SellerId,
                request.Price,
                request.IsNegotiable,
                request.IsUrgent,
                request.ContactPhone,
                request.ContactEmail
            );

            // Create command
            CreateAdvertisementCommand command = new(dto, correlationId);

            // Execute command
            Result<Guid> result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.ToResponse(localizationService);
        })
        .WithName("CreateAdvertisement")
        .WithSummary("Create a new advertisement")
        .WithDescription("Creates a new advertisement for a product")
        .WithOpenApi()
        .Produces<Guid>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
        // .RequireAuthorization(); // TODO: Add when auth is implemented
    }
}

/// <summary>
/// Create advertisement request DTO for API
/// </summary>
public record CreateAdvertisementRequest(
    string Title,
    string Description,
    Guid ProductId,
    Guid SellerId,
    Money Price,
    Guid CorrelationId = default,
    bool IsNegotiable = false,
    bool IsUrgent = false,
    string? ContactPhone = null,
    string? ContactEmail = null);
