using MediaTR.SharedKernel.ResultAndError;

namespace MediaTR.Domain.Errors;

public static class ProductErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Product.NotFound",
        "The product with the specified identifier was not found",
        "Product.NotFound");

    public static readonly Error InvalidName = Error.Validation(
        "Product.InvalidName",
        "Product name cannot be empty",
        "Validation.Required",
        "Product name");

    public static readonly Error InvalidPrice = Error.Validation(
        "Product.InvalidPrice",
        "Product price must be greater than zero",
        "Validation.Range",
        "Price", 0.01, double.MaxValue);

    public static readonly Error InvalidSku = Error.Validation(
        "Product.InvalidSku",
        "Product SKU is required and must be unique",
        "Validation.Required",
        "SKU");

    public static readonly Error InvalidStock = Error.Validation(
        "Product.InvalidStock",
        "Product stock quantity cannot be negative",
        "Validation.Range",
        "Stock", 0, int.MaxValue);

    public static readonly Error InvalidWeight = Error.Validation(
        "Product.InvalidWeight",
        "Product weight cannot be negative",
        "Validation.Range",
        "Weight", 0, double.MaxValue);

    public static readonly Error CannotDelete = Error.Validation(
        "Product.CannotDelete",
        "Product cannot be deleted as it has active orders",
        "Product.CannotDelete");

    //public static readonly Error InsufficientStock = Error.Validation(
    //    "Product.InsufficientStock",
    //    "Insufficient stock quantity available");

    public static readonly Error Discontinued = Error.Validation(
        "Product.Discontinued",
        "Product has been discontinued and is no longer available",
        "Product.Inactive");

    public static Error CategoryNotFound(Guid categoryId) => Error.NotFound(
        "Product.CategoryNotFound",
        $"Category with Id {categoryId} was not found",
        "Category.NotFound");

    public static Error SkuAlreadyExists(string sku) => Error.Conflict(
        "Product.SkuAlreadyExists",
        $"A product with SKU '{sku}' already exists",
        "Product.SkuAlreadyExists");

    public static Error InsufficientStock(string productName, int available, int requested) => Error.Validation(
        "Product.InsufficientStock",
        $"Product '{productName}' has only {available} items available, but {requested} were requested",
        "Product.InsufficientStock");
}