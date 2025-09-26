using MediaTR.Domain.ValueObjects;

namespace MediaTR.Application.Features.Advertisements.DTOs;

public record CreateAdvertisementResult(
    Guid Id,
    string Title,
    Guid ProductId,
    Guid SellerId,
    Money Price,
    string Status,
    bool IsNegotiable,
    bool IsUrgent
);