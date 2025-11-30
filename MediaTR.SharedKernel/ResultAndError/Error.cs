namespace MediaTR.SharedKernel.ResultAndError;

public sealed record Error
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);
    public static readonly Error NullValue = new("Error.NullValue", "Null value was provided", ErrorType.Failure);

    private Error(string code, string description, ErrorType type, string? localizationKey = null, object[]? localizationArgs = null)
    {
        Code = code;
        Description = description;
        Type = type;
        LocalizationKey = localizationKey;
        LocalizationArgs = localizationArgs;
    }

    public string Code { get; }
    public string Description { get; }
    public ErrorType Type { get; }

    /// <summary>
    /// Resource key for localization (e.g., "Order.NotFound")
    /// If provided, localization service will use this to get localized message
    /// </summary>
    public string? LocalizationKey { get; }

    /// <summary>
    /// Format arguments for localized string (e.g., field name, min/max values)
    /// </summary>
    public object[]? LocalizationArgs { get; }

    public static Error Failure(string code, string description, string? localizationKey = null, params object[] localizationArgs) =>
        new(code, description, ErrorType.Failure, localizationKey, localizationArgs.Length > 0 ? localizationArgs : null);

    public static Error Validation(string code, string description, string? localizationKey = null, params object[] localizationArgs) =>
        new(code, description, ErrorType.Validation, localizationKey, localizationArgs.Length > 0 ? localizationArgs : null);

    public static Error Problem(string code, string description, string? localizationKey = null, params object[] localizationArgs) =>
        new(code, description, ErrorType.Problem, localizationKey, localizationArgs.Length > 0 ? localizationArgs : null);

    public static Error NotFound(string code, string description, string? localizationKey = null, params object[] localizationArgs) =>
        new(code, description, ErrorType.NotFound, localizationKey, localizationArgs.Length > 0 ? localizationArgs : null);

    public static Error Conflict(string code, string description, string? localizationKey = null, params object[] localizationArgs) =>
        new(code, description, ErrorType.Conflict, localizationKey, localizationArgs.Length > 0 ? localizationArgs : null);

    public static Error Unauthorized(string code, string description, string? localizationKey = null, params object[] localizationArgs) =>
        new(code, description, ErrorType.Unauthorized, localizationKey, localizationArgs.Length > 0 ? localizationArgs : null);

    public static Error Forbidden(string code, string description, string? localizationKey = null, params object[] localizationArgs) =>
        new(code, description, ErrorType.Forbidden, localizationKey, localizationArgs.Length > 0 ? localizationArgs : null);
}

public enum ErrorType
{
    Failure = 0,
    Validation = 1,
    Problem = 2,
    NotFound = 3,
    Conflict = 4,
    Unauthorized = 5,
    Forbidden = 6
}