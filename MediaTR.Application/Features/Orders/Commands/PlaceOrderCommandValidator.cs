using FluentValidation;
using MediaTR.Domain.ValueObjects;

namespace MediaTR.Application.Features.Orders.Commands;

/// <summary>
/// Validator for PlaceOrderCommand using FluentValidation
/// </summary>
public sealed class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderCommandValidator()
    {
        RuleFor(x => x.Request.UserId)
            .NotEmpty()
            .WithMessage("User ID is required")
            .NotEqual(Guid.Empty)
            .WithMessage("User ID cannot be empty GUID");

        RuleFor(x => x.Request.OrderItems)
            .NotNull()
            .WithMessage("Order items cannot be null")
            .NotEmpty()
            .WithMessage("Order must contain at least one item")
            .Must(items => items.Count <= 100)
            .WithMessage("Order cannot contain more than 100 items");

        RuleForEach(x => x.Request.OrderItems)
            .SetValidator(new OrderItemRequestValidator());

        RuleFor(x => x.Request.ShippingAddress)
            .NotNull()
            .WithMessage("Shipping address is required")
            .SetValidator(new AddressValidator());

        RuleFor(x => x.Request.BillingAddress)
            .NotNull()
            .WithMessage("Billing address is required")
            .SetValidator(new AddressValidator());

        RuleFor(x => x.Request.PaymentMethod)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.Request.PaymentMethod))
            .WithMessage("Payment method cannot exceed 50 characters");

        RuleFor(x => x.Request.Notes)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Request.Notes))
            .WithMessage("Notes cannot exceed 500 characters");
    }
}

/// <summary>
/// Validator for OrderItemRequest
/// </summary>
public sealed class OrderItemRequestValidator : AbstractValidator<OrderItemRequest>
{
    public OrderItemRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Product ID is required")
            .NotEqual(Guid.Empty)
            .WithMessage("Product ID cannot be empty GUID");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0")
            .LessThanOrEqualTo(1000)
            .WithMessage("Quantity cannot exceed 1000 per item");

        RuleFor(x => x.UnitPrice)
            .NotNull()
            .WithMessage("Unit price is required");

        RuleFor(x => x.UnitPrice.Amount)
            .GreaterThan(0)
            .When(x => x.UnitPrice != null)
            .WithMessage("Unit price must be greater than 0")
            .LessThan(1_000_000)
            .When(x => x.UnitPrice != null)
            .WithMessage("Unit price cannot exceed 1,000,000");
    }
}

/// <summary>
/// Validator for Address value object
/// </summary>
public sealed class AddressValidator : AbstractValidator<Address>
{
    public AddressValidator()
    {
        RuleFor(x => x.Street)
            .NotEmpty()
            .WithMessage("Street is required")
            .MaximumLength(200)
            .WithMessage("Street cannot exceed 200 characters");

        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage("City is required")
            .MaximumLength(100)
            .WithMessage("City cannot exceed 100 characters");

        RuleFor(x => x.State)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.State))
            .WithMessage("State cannot exceed 100 characters");

        RuleFor(x => x.Country)
            .NotEmpty()
            .WithMessage("Country is required")
            .MaximumLength(100)
            .WithMessage("Country cannot exceed 100 characters");

        RuleFor(x => x.PostalCode)
            .NotEmpty()
            .WithMessage("Zip code is required")
            .Matches(@"^\d{5}(-\d{4})?$")
            .WithMessage("Invalid zip code format (expected: 12345 or 12345-6789)");
    }
}
