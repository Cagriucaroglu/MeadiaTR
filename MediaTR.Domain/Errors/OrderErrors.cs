using MediaTR.SharedKernel.ResultAndError;

namespace MediaTR.Domain.Errors;

public static class OrderErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Order.NotFound",
        "The order with the specified identifier was not found",
        "Order.NotFound");

    public static readonly Error InvalidOrderItems = Error.Validation(
        "Order.InvalidOrderItems",
        "Order must contain at least one item",
        "Validation.Required",
        "Order items");

    public static readonly Error InvalidTotalAmount = Error.Validation(
        "Order.InvalidTotalAmount",
        "Order total amount must be greater than zero",
        "Validation.Range",
        "Total amount", 0.01, double.MaxValue);

    public static readonly Error CannotConfirm = Error.Validation(
        "Order.CannotConfirm",
        "Only pending orders can be confirmed",
        "Order.InvalidStatus");

    public static readonly Error CannotProcess = Error.Validation(
        "Order.CannotProcess",
        "Only confirmed orders can be processed",
        "Order.InvalidStatus");

    public static readonly Error CannotShip = Error.Validation(
        "Order.CannotShip",
        "Only processing orders can be shipped",
        "Order.InvalidStatus");

    public static readonly Error CannotDeliver = Error.Validation(
        "Order.CannotDeliver",
        "Only shipped orders can be delivered",
        "Order.InvalidStatus");

    public static readonly Error CannotCancel = Error.Validation(
        "Order.CannotCancel",
        "Order cannot be cancelled in current status",
        "Order.CannotCancel");

    public static readonly Error TrackingNumberRequired = Error.Validation(
        "Order.TrackingNumberRequired",
        "Tracking number is required for shipping",
        "Validation.Required",
        "Tracking number");

    public static Error UserNotFound(Guid userId) => Error.NotFound(
        "Order.UserNotFound",
        $"User with Id {userId} was not found",
        "User.NotFound");

    public static Error ProductNotFound(Guid productId) => Error.NotFound(
        "Order.ProductNotFound",
        $"Product with Id {productId} was not found",
        "Product.NotFound");

    public static Error InsufficientStock(string productName) => Error.Validation(
        "Order.InsufficientStock",
        $"Insufficient stock for product {productName}",
        "Product.InsufficientStock");
}