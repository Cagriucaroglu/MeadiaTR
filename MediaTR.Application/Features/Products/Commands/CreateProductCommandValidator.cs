using FluentValidation;

namespace MediaTR.Application.Features.Products.Commands;

/// <summary>
/// Validator for CreateProductCommand
/// </summary>
public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Request.Name)
            .NotEmpty()
            .WithMessage("Product name is required")
            .MaximumLength(200)
            .WithMessage("Product name cannot exceed 200 characters")
            .MinimumLength(3)
            .WithMessage("Product name must be at least 3 characters");

        RuleFor(x => x.Request.Description)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.Request.Description))
            .WithMessage("Product description cannot exceed 2000 characters");

        RuleFor(x => x.Request.CategoryId)
            .NotEmpty()
            .WithMessage("Category ID is required")
            .NotEqual(Guid.Empty)
            .WithMessage("Category ID cannot be empty GUID");

        RuleFor(x => x.Request.Price)
            .NotNull()
            .WithMessage("Price is required");

        RuleFor(x => x.Request.Price.Amount)
            .GreaterThan(0)
            .When(x => x.Request.Price != null)
            .WithMessage("Price must be greater than 0")
            .LessThan(1_000_000)
            .When(x => x.Request.Price != null)
            .WithMessage("Price cannot exceed 1,000,000");

        RuleFor(x => x.Request.Sku)
            .NotEmpty()
            .WithMessage("SKU is required")
            .MaximumLength(50)
            .WithMessage("SKU cannot exceed 50 characters")
            .Matches(@"^[A-Z0-9\-]+$")
            .WithMessage("SKU must contain only uppercase letters, numbers, and hyphens");

        RuleFor(x => x.Request.StockQuantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Stock quantity cannot be negative")
            .LessThan(1_000_000)
            .WithMessage("Stock quantity cannot exceed 1,000,000");

        RuleFor(x => x.Request.Weight)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Weight cannot be negative")
            .LessThan(10_000)
            .WithMessage("Weight cannot exceed 10,000 kg");
    }
}
