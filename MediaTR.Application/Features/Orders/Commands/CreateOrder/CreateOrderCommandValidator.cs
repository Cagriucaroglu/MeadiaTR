using FluentValidation;

namespace MediaTR.Application.Features.Orders.Commands.CreateOrder;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        // Shipping Address
        RuleFor(x => x.ShippingStreet)
            .NotEmpty().WithMessage("Order.ShippingStreet.Required")
            .MaximumLength(200).WithMessage("Order.ShippingStreet.TooLong");

        RuleFor(x => x.ShippingCity)
            .NotEmpty().WithMessage("Order.ShippingCity.Required")
            .MaximumLength(100).WithMessage("Order.ShippingCity.TooLong");

        RuleFor(x => x.ShippingCountry)
            .NotEmpty().WithMessage("Order.ShippingCountry.Required")
            .MaximumLength(100).WithMessage("Order.ShippingCountry.TooLong");

        RuleFor(x => x.ShippingPostalCode)
            .NotEmpty().WithMessage("Order.ShippingPostalCode.Required")
            .MaximumLength(20).WithMessage("Order.ShippingPostalCode.TooLong");

        // Payment Method
        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage("Order.PaymentMethod.Required")
            .Must(pm => new[] { "CreditCard", "PayPal", "CashOnDelivery" }.Contains(pm))
            .WithMessage("Order.PaymentMethod.Invalid");

        // Billing Address (only if provided)
        When(x => !string.IsNullOrWhiteSpace(x.BillingStreet), () =>
        {
            RuleFor(x => x.BillingCity)
                .NotEmpty().WithMessage("Order.BillingCity.Required");

            RuleFor(x => x.BillingCountry)
                .NotEmpty().WithMessage("Order.BillingCountry.Required");
        });

        // Notes
        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Order.Notes.TooLong");
    }
}
