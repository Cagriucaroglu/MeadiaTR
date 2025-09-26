using MediaTR.Domain.ValueObjects;

namespace MediaTR.Application.Features.Products.DTOs;

public record CreateProductResult(
    Guid Id,
    string Name,
    string Slug,
    Money Price,
    string Sku,
    bool IsActive
);