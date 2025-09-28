using MediaTR.SharedKernel.ResultAndError;

namespace MediaTR.Domain.Errors;

public static class AdvertisementErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Advertisement.NotFound",
        "The advertisement with the specified identifier was not found");

    public static readonly Error InvalidTitle = Error.Validation(
        "Advertisement.InvalidTitle",
        "Advertisement title cannot be empty");

    public static readonly Error InvalidDescription = Error.Validation(
        "Advertisement.InvalidDescription",
        "Advertisement description cannot be empty");

    public static readonly Error InvalidPrice = Error.Validation(
        "Advertisement.InvalidPrice",
        "Advertisement price must be greater than zero");

    public static readonly Error InvalidContactInfo = Error.Validation(
        "Advertisement.InvalidContactInfo",
        "At least one contact method (phone or email) must be provided");

    public static readonly Error CannotApprove = Error.Validation(
        "Advertisement.CannotApprove",
        "Only pending advertisements can be approved");

    public static readonly Error CannotReject = Error.Validation(
        "Advertisement.CannotReject",
        "Only pending or approved advertisements can be rejected");

    public static readonly Error CannotPublish = Error.Validation(
        "Advertisement.CannotPublish",
        "Only approved advertisements can be published");

    public static readonly Error AlreadyExpired = Error.Validation(
        "Advertisement.AlreadyExpired",
        "Advertisement has already expired");

    public static readonly Error CannotEdit = Error.Validation(
        "Advertisement.CannotEdit",
        "Published advertisements cannot be edited");

    public static readonly Error CannotDelete = Error.Validation(
        "Advertisement.CannotDelete",
        "Published advertisements cannot be deleted");

    public static readonly Error UnauthorizedAccess = Error.Forbidden(
        "Advertisement.UnauthorizedAccess",
        "You are not authorized to access this advertisement");

    public static Error ProductNotFound(Guid productId) => Error.NotFound(
        "Advertisement.ProductNotFound",
        $"Product with Id {productId} was not found");

    public static Error SellerNotFound(Guid sellerId) => Error.NotFound(
        "Advertisement.SellerNotFound",
        $"Seller with Id {sellerId} was not found");

    public static Error UnauthorizedSeller(Guid userId, Guid sellerId) => Error.Forbidden(
        "Advertisement.UnauthorizedSeller",
        $"User {userId} is not authorized to create advertisements for seller {sellerId}");

    public static Error ExpiredAdvertisement(DateTime expiredAt) => Error.Validation(
        "Advertisement.ExpiredAdvertisement",
        $"Advertisement expired on {expiredAt:yyyy-MM-dd}");
}