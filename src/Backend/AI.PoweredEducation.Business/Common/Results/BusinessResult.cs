using AI.PoweredEducation.Business.Common.Exceptions;
using AI.PoweredEducation.Business.Authentication.Exceptions;
using AI.PoweredEducation.Core.Common;
using FluentValidation;

namespace AI.PoweredEducation.Business.Common.Results;

internal static class BusinessResult
{
    public static async Task<Result<TValue>> FromAsync<TValue>(Func<Task<TValue>> action)
    {
        try
        {
            return Result.Success(await action());
        }
        catch (Exception exception) when (TryMap(exception, out var error))
        {
            return Result.Failure<TValue>(error);
        }
    }

    public static async Task<Result> FromAsync(Func<Task> action)
    {
        try
        {
            await action();
            return Result.Success();
        }
        catch (Exception exception) when (TryMap(exception, out var error))
        {
            return Result.Failure(error);
        }
    }

    private static bool TryMap(Exception exception, out Error error)
    {
        error = exception switch
        {
            ValidationException validationException => Error.Validation(
                "Validation.Failed",
                "Request validation failed.",
                validationException.Errors
                    .Select(validationFailure => validationFailure.ErrorMessage)
                    .ToArray()),
            ResourceNotFoundException notFoundException => Error.NotFound(
                "Resource.NotFound",
                notFoundException.Message),
            BusinessRuleException businessRuleException => Error.Conflict(
                "BusinessRule.Violation",
                businessRuleException.Message),
            AuthenticationServiceException authenticationException => ToAuthenticationError(authenticationException),
            _ => null!
        };

        return error is not null;
    }

    private static Error ToAuthenticationError(AuthenticationServiceException exception) =>
        exception.ErrorCode switch
        {
            AuthenticationErrorCode.InvalidCredentials => Error.Unauthorized(
                "Authentication.InvalidCredentials",
                exception.Message),
            AuthenticationErrorCode.InvalidRefreshToken => Error.Unauthorized(
                "Authentication.InvalidRefreshToken",
                exception.Message),
            AuthenticationErrorCode.RegistrationFailed => Error.Validation(
                "Authentication.RegistrationFailed",
                exception.Message,
                exception.Details),
            _ => Error.Failure(
                "Authentication.Failed",
                exception.Message)
        };
}
