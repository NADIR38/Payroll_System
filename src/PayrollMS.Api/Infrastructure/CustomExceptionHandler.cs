using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayrollMS.Application.Common.Exceptions;
using PayrollMS.Domain.Exceptions;

namespace PayrollMS.Api.Infrastructure;

public class CustomExceptionHandler : IExceptionHandler
{
    private readonly ILogger<CustomExceptionHandler> _logger;

    public CustomExceptionHandler(ILogger<CustomExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails = exception switch
        {
            ValidationException validationEx => CreateValidationProblemDetails(validationEx, httpContext),
            FluentValidation.ValidationException fluentEx => CreateFluentValidationProblemDetails(fluentEx, httpContext),
            NotFoundException notFoundEx => CreateProblemDetails(httpContext, StatusCodes.Status404NotFound, "Not Found", notFoundEx.Message),
            BusinessRuleViolationException businessEx => CreateBusinessRuleProblemDetails(businessEx, httpContext),
            DbUpdateConcurrencyException => CreateProblemDetails(httpContext, StatusCodes.Status409Conflict, "Conflict", "The record has been modified by another user. Please refresh and try again."),
            _ => CreateProblemDetails(httpContext, StatusCodes.Status500InternalServerError, "Internal Server Error", "An unexpected error occurred.")
        };

        if (problemDetails.Status == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning("API exception handled: {Message}", exception.Message);
        }

        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, problemDetails.GetType(), cancellationToken);

        return true;
    }

    private ProblemDetails CreateProblemDetails(HttpContext context, int status, string title, string detail)
    {
        return new ProblemDetails
        {
            Status = status,
            Type = $"https://httpstatuses.io/{status}",
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };
    }

    private ProblemDetails CreateValidationProblemDetails(ValidationException ex, HttpContext context)
    {
        var problemDetails = new HttpValidationProblemDetails(ex.Errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Type = "https://httpstatuses.io/400",
            Title = "Bad Request",
            Detail = "One or more validation errors occurred.",
            Instance = context.Request.Path
        };

        return problemDetails;
    }

    private ProblemDetails CreateFluentValidationProblemDetails(FluentValidation.ValidationException ex, HttpContext context)
    {
        var errors = ex.Errors
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());

        return new HttpValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Type = "https://httpstatuses.io/400",
            Title = "Bad Request",
            Detail = "One or more validation errors occurred.",
            Instance = context.Request.Path
        };
    }

    private ProblemDetails CreateBusinessRuleProblemDetails(BusinessRuleViolationException ex, HttpContext context)
    {
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status422UnprocessableEntity,
            Type = "https://httpstatuses.io/422",
            Title = "Business Rule Violation",
            Detail = ex.Message,
            Instance = context.Request.Path
        };

        problemDetails.Extensions.Add("errorCode", ex.ErrorCode);

        return problemDetails;
    }
}
