using MediaTR.SharedKernel.ResultAndError;

namespace MediaTR.ApiService.Extensions;

public static class ResultExtensions
{
    public static Microsoft.AspNetCore.Http.IResult ToResponse<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        var errorResponse = new { error = result.Error.Code, message = result.Error.Description };

        return result.Error.Type switch
        {
            ErrorType.NotFound => Results.NotFound(errorResponse),
            ErrorType.Validation => Results.BadRequest(errorResponse),
            ErrorType.Conflict => Results.Conflict(errorResponse),
            ErrorType.Unauthorized => Results.Unauthorized(),
            ErrorType.Forbidden => Results.Forbid(),
            _ => Results.Problem(
                detail: result.Error.Description,
                title: result.Error.Code,
                statusCode: 500)
        };
    }

    public static Microsoft.AspNetCore.Http.IResult ToResponse(this Result result)
    {
        if (result.IsSuccess)
        {
            return Results.NoContent();
        }

        var errorResponse = new { error = result.Error.Code, message = result.Error.Description };

        return result.Error.Type switch
        {
            ErrorType.NotFound => Results.NotFound(errorResponse),
            ErrorType.Validation => Results.BadRequest(errorResponse),
            ErrorType.Conflict => Results.Conflict(errorResponse),
            ErrorType.Unauthorized => Results.Unauthorized(),
            ErrorType.Forbidden => Results.Forbid(),
            _ => Results.Problem(
                detail: result.Error.Description,
                title: result.Error.Code,
                statusCode: 500)
        };
    }

    public static Microsoft.AspNetCore.Http.IResult ToCreatedResponse<T>(this Result<T> result, string location)
    {
        if (result.IsSuccess)
        {
            return Results.Created(location, result.Value);
        }

        return result.ToResponse();
    }
}