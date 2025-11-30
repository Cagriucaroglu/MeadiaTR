using MediaTR.SharedKernel.Localization;
using MediaTR.SharedKernel.ResultAndError;

namespace MediaTR.ApiService.Extensions;

public static class ResultExtensions
{

    /// <summary>
    /// Converts Result to HTTP response WITH localization support
    /// Uses ILocalizationService to get localized error messages based on current culture
    /// </summary>
    public static Microsoft.AspNetCore.Http.IResult ToResponse<T>(
        this Result<T> result,
        ILocalizationService localizationService)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        var error = result.Error;
        var localizedMessage = GetLocalizedMessage(error, localizationService);
        var errorResponse = new { error = error.Code, message = localizedMessage };

        return error.Type switch
        {
            ErrorType.NotFound => Results.NotFound(errorResponse),
            ErrorType.Validation => Results.BadRequest(errorResponse),
            ErrorType.Conflict => Results.Conflict(errorResponse),
            ErrorType.Unauthorized => Results.Unauthorized(),
            ErrorType.Forbidden => Results.Forbid(),
            _ => Results.Problem(
                detail: localizedMessage,
                title: error.Code,
                statusCode: 500)
        };
    }

    /// <summary>
    /// Gets the localized error message if LocalizationKey is provided,
    /// otherwise returns the default Description
    /// </summary>
    private static string GetLocalizedMessage(Error error, ILocalizationService localizationService)
    {
        // If no localization key, return default description
        if (string.IsNullOrEmpty(error.LocalizationKey))
        {
            return error.Description;
        }

        // Get localized message using key and args
        if (error.LocalizationArgs != null && error.LocalizationArgs.Length > 0)
        {
            return localizationService.GetString(error.LocalizationKey, error.LocalizationArgs);
        }

        return localizationService.GetString(error.LocalizationKey);
    }
}