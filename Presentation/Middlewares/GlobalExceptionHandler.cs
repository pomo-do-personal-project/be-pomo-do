using be_pomo_do.Core.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace be_pomo_do.Presentation.Middlewares;

/// <summary>
/// Handles exceptions globally using the modern .NET IExceptionHandler pattern.
/// </summary>
/// <param name="problemDetailsService">The problem details service.</param>
/// <param name="logger">The logger instance.</param>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalExceptionHandler"/> class.
    /// </summary>
    /// <param name="problemDetailsService">The problem details service.</param>
    /// <param name="logger">The logger instance.</param>
    public GlobalExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<GlobalExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService ?? throw new ArgumentNullException(nameof(problemDetailsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        int statusCode;
        string title;
        string detail;

        if (exception is DomainException domainEx)
        {
            statusCode = StatusCodes.Status400BadRequest;
            title = "Domain Rule Violation";
            detail = domainEx.Message;
        }
        else
        {
            statusCode = StatusCodes.Status500InternalServerError;
            title = "Internal Server Error";
            detail = "An unexpected error occurred while processing your request.";
        }

        httpContext.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception
        });
    }
}
