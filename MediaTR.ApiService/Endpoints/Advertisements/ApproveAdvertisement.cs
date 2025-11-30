using MediatR;
using MediaTR.ApiService.Endpoints;
using MediaTR.ApiService.Extensions;
using MediaTR.Application.Features.Advertisements.Commands;
using MediaTR.SharedKernel.Localization;
using MediaTR.SharedKernel.ResultAndError;
using Microsoft.AspNetCore.Mvc;

namespace MediaTR.ApiService.Endpoints.Advertisements;

/// <summary>
/// Approve advertisement endpoint with outbox pattern for domain events
/// </summary>
internal sealed class ApproveAdvertisement : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/advertisements/{advertisementId:guid}/approve", async (
            Guid advertisementId,
            ISender sender,
            ILocalizationService localizationService,
            CancellationToken cancellationToken) =>
        {
            Guid correlationId = Guid.NewGuid();

            // Create DTO
            ApproveAdvertisementDto dto = new(advertisementId);

            // Create command
            ApproveAdvertisementCommand command = new(dto, correlationId);

            // Execute command
            Result<Guid> result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.ToResponse(localizationService);
        })
        .WithName("ApproveAdvertisement")
        .WithSummary("Approve an advertisement")
        .WithDescription("Approves a pending advertisement and publishes AdvertisementPublished domain event")
        .WithOpenApi()
        .Produces<Guid>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
        // .RequireAuthorization(); // TODO: Add when auth is implemented - admin only
    }
}
