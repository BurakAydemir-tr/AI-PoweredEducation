using AI.PoweredEducation.Core.Common;
using Microsoft.AspNetCore.Mvc;

namespace AI.PoweredEducation.API.Results;

internal static class ResultExtensions
{
    public static ActionResult<TValue> ToActionResult<TValue>(
        this Result<TValue> result,
        ControllerBase controller)
    {
        if (result.IsSuccess)
        {
            return controller.Ok(result.Value);
        }

        return ToProblemDetails(result.Error!, controller);
    }

    public static IActionResult ToNoContentActionResult(
        this Result result,
        ControllerBase controller)
    {
        if (result.IsSuccess)
        {
            return controller.NoContent();
        }

        return ToProblemDetails(result.Error!, controller);
    }

    private static ObjectResult ToProblemDetails(Error error, ControllerBase controller)
    {
        var statusCode = ToStatusCode(error.Type);
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = error.Code,
            Detail = error.Message
        };

        if (error.Details is { Count: > 0 })
        {
            problemDetails.Extensions["errors"] = error.Details;
        }

        return controller.StatusCode(statusCode, problemDetails);
    }

    private static int ToStatusCode(ErrorType errorType) =>
        errorType switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.ExternalService => StatusCodes.Status502BadGateway,
            _ => StatusCodes.Status400BadRequest
        };
}
