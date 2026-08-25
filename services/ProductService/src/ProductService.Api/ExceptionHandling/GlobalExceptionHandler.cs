using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Exceptions;

namespace ProductService.Api.ExceptionHandling;

public class GlobalExceptionHandler : IExceptionHandler
{
    private const string UnexpectedErrorDetail = "An unexpected error occurred. Please try again later.";

    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<GlobalExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, title, detail) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Resource not found", exception.Message),
            InvalidCategoryReferenceException => (StatusCodes.Status400BadRequest, "Invalid category reference", exception.Message),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred", UnexpectedErrorDetail)
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception while processing {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
        }
        else
        {
            _logger.LogWarning(exception, "Handled exception while processing {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = statusCode;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = httpContext.Request.Path
            }
        });
    }
}
