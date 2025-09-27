using MediaTR.Application.Abstractions.Messaging;
using MediaTR.Domain.ValueObjects;

namespace MediaTR.Application.Features.Products.Queries;

public record GetProductQuery(Guid Id) : IQuery<GetProductResult>;

public record GetProductResult(
    Guid Id,
    string Name,
    string Description,
    string Slug,
    Guid CategoryId,
    Money Price,
    int StockQuantity,
    string Sku,
    bool IsActive,
    bool IsFeatured,
    bool IsInStock,
    string? MainImageUrl
);