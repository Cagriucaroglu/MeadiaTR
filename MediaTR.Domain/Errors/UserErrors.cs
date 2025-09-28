using MediaTR.SharedKernel.ResultAndError;

namespace MediaTR.Domain.Errors;

public static class UserErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "User.NotFound",
        "The user with the specified identifier was not found");

    public static readonly Error InvalidEmail = Error.Validation(
        "User.InvalidEmail",
        "The email address format is invalid");

    public static readonly Error InvalidFirstName = Error.Validation(
        "User.InvalidFirstName",
        "First name cannot be empty");

    public static readonly Error InvalidLastName = Error.Validation(
        "User.InvalidLastName",
        "Last name cannot be empty");

    public static readonly Error InvalidPhoneNumber = Error.Validation(
        "User.InvalidPhoneNumber",
        "Phone number format is invalid");

    public static readonly Error InvalidPassword = Error.Validation(
        "User.InvalidPassword",
        "Password must be at least 8 characters long and contain uppercase, lowercase, number and special character");

    public static readonly Error AccountInactive = Error.Validation(
        "User.AccountInactive",
        "User account is inactive");

    public static readonly Error AccountLocked = Error.Validation(
        "User.AccountLocked",
        "User account is locked due to too many failed login attempts");

    public static readonly Error InvalidCredentials = Error.Unauthorized(
        "User.InvalidCredentials",
        "Invalid email or password");

    public static readonly Error UnauthorizedAccess = Error.Forbidden(
        "User.UnauthorizedAccess",
        "You are not authorized to perform this action");

    public static readonly Error CannotDeleteSelf = Error.Validation(
        "User.CannotDeleteSelf",
        "You cannot delete your own account");

    public static Error EmailAlreadyExists(string email) => Error.Conflict(
        "User.EmailAlreadyExists",
        $"A user with email '{email}' already exists");

    public static Error PhoneAlreadyExists(string phone) => Error.Conflict(
        "User.PhoneAlreadyExists",
        $"A user with phone number '{phone}' already exists");

    public static Error NotFoundByEmail(string email) => Error.NotFound(
        "User.NotFoundByEmail",
        $"User with email '{email}' was not found");

    public static Error HasActiveOrders(int orderCount) => Error.Validation(
        "User.HasActiveOrders",
        $"User cannot be deleted as they have {orderCount} active orders");

    public static Error HasActiveAdvertisements(int adCount) => Error.Validation(
        "User.HasActiveAdvertisements",
        $"User cannot be deleted as they have {adCount} active advertisements");
}