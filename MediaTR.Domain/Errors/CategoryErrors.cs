using MediaTR.SharedKernel.ResultAndError;

namespace MediaTR.Domain.Errors;

public static class CategoryErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Category.NotFound",
        "The category with the specified identifier was not found");

    public static readonly Error InvalidName = Error.Validation(
        "Category.InvalidName",
        "Category name cannot be empty");

    public static readonly Error InvalidSlug = Error.Validation(
        "Category.InvalidSlug",
        "Category slug cannot be empty and must be unique");

    public static readonly Error InvalidSortOrder = Error.Validation(
        "Category.InvalidSortOrder",
        "Category sort order cannot be negative");

    public static readonly Error CannotDelete = Error.Validation(
        "Category.CannotDelete",
        "Category cannot be deleted as it has products or subcategories");

    public static readonly Error CircularReference = Error.Validation(
        "Category.CircularReference",
        "Category cannot be its own parent or create a circular reference");

    public static readonly Error MaxDepthExceeded = Error.Validation(
        "Category.MaxDepthExceeded",
        "Category hierarchy depth cannot exceed maximum allowed levels");

    public static Error ParentNotFound(Guid parentId) => Error.NotFound(
        "Category.ParentNotFound",
        $"Parent category with Id {parentId} was not found");

    public static Error SlugAlreadyExists(string slug) => Error.Conflict(
        "Category.SlugAlreadyExists",
        $"A category with slug '{slug}' already exists");

    public static Error NameAlreadyExists(string name) => Error.Conflict(
        "Category.NameAlreadyExists",
        $"A category with name '{name}' already exists in the same parent category");

    public static Error HasActiveProducts(int productCount) => Error.Validation(
        "Category.HasActiveProducts",
        $"Category cannot be deleted as it has {productCount} active products");

    public static Error HasSubcategories(int subcategoryCount) => Error.Validation(
        "Category.HasSubcategories",
        $"Category cannot be deleted as it has {subcategoryCount} subcategories");
}