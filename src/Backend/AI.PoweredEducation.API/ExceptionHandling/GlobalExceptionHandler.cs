using AI.PoweredEducation.Business.Authentication.Exceptions;
using AI.PoweredEducation.Business.Common.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AI.PoweredEducation.API.ExceptionHandling;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;

    public GlobalExceptionHandler(IProblemDetailsService problemDetailsService)
    {
        _problemDetailsService = problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails = exception switch
        {
            ValidationException validationException => CreateValidationProblem(validationException),
            AuthenticationServiceException authenticationException =>
                CreateAuthenticationProblem(authenticationException),
            ResourceNotFoundException notFoundException => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = notFoundException.Message
            },
            BusinessRuleException businessRuleException => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = businessRuleException.Message
            },
            UnauthorizedAccessException unauthorizedException => new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = unauthorizedException.Message
            },
            _ => null
        };

        if (problemDetails is null)
        {
            return false;
        }

        httpContext.Response.StatusCode = problemDetails.Status!.Value;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception
        });
    }

    private static ProblemDetails CreateValidationProblem(ValidationException exception)
    {
        var errors = exception.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).ToArray());

        return new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation failed.",
            Extensions = { ["errors"] = errors }
        };
    }

    private static ProblemDetails CreateAuthenticationProblem(
        AuthenticationServiceException exception)
    {
        var status = exception.ErrorCode == AuthenticationErrorCode.RegistrationFailed
            ? StatusCodes.Status400BadRequest
            : StatusCodes.Status401Unauthorized;

        var problem = new ProblemDetails
        {
            Status = status,
            Title = exception.Message
        };

        if (exception.Details.Count > 0)
        {
            problem.Extensions["errors"] = exception.Details;
        }

        return problem;
    }
}
