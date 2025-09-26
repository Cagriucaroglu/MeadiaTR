using MediatR;
using MediaTR.Application.Features.Advertisements.DTOs;
using MediaTR.Domain.ValueObjects;

namespace MediaTR.Application.Features.Advertisements.Commands;

public record CreateAdvertisementCommand(
    string Title,
    string Description,
    Guid ProductId,
    Guid SellerId,
    Money Price,
    bool IsNegotiable = false,
    bool IsUrgent = false,
    string? ContactPhone = null,
    string? ContactEmail = null
) : IRequest<CreateAdvertisementResult>;