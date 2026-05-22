using FluentValidation;
using Inventory.Domain;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Api;

public class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problem = exception switch
        {
            ValidationException vex => CreateValidationProblemDetails(vex),
            DomainException dex => CreateDomainProblemDetails(dex),
            _ => CreateUnexpectedProblemDetails()
        };

        httpContext.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;

        var logLevel = problem.Status >= 500 ? LogLevel.Error : LogLevel.Warning;
        logger.Log(logLevel, exception,
            "Request {Method} {Path} failed: {ProblemTitle} (status {Status})",
            httpContext.Request.Method, httpContext.Request.Path, problem.Title, problem.Status);

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception
        });
    }

    private static ValidationProblemDetails CreateValidationProblemDetails(ValidationException exception)
    {
        var errors = exception.Errors
            .GroupBy(x => x.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        return new ValidationProblemDetails(errors)
        {
            Title = "Validation failed",
            Status = StatusCodes.Status400BadRequest,
        };
    }

    private static ProblemDetails CreateDomainProblemDetails(DomainException exception)
    {
        return new ProblemDetails
        {
            Title = "Domain rule violation",
            Detail = exception.Message,
            Status = StatusCodes.Status422UnprocessableEntity
        };
    }

    private static ProblemDetails CreateUnexpectedProblemDetails()
    {
        return new ProblemDetails
        {
            Title = "Unexpected problem",
            Detail = "An unexpected error occured",
            Status = StatusCodes.Status500InternalServerError
        };
    }
}
