using System.Text.Json;
using ConstructionAssetAPI.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace ConstructionAssetAPI.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        var (status, title) = ex switch
        {
            NotFoundException  => (StatusCodes.Status404NotFound, "Resource not found"),
            ConflictException  => (StatusCodes.Status409Conflict, "Conflict"),
            ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden"),
            _                  => (StatusCodes.Status500InternalServerError, "Internal server error")
        };

        var problem = new ProblemDetails
        {
            Status   = status,
            Title    = title,
            Detail   = status == StatusCodes.Status500InternalServerError
                       ? "An unexpected error occurred. Quote the correlation id when reporting."
                       : ex.Message,
            Instance = context.Request.Path
        };

        // Correlation id: always emit on 500; harmless to emit on others too.
        var correlationId = Guid.NewGuid().ToString();
        problem.Extensions["correlationId"] = correlationId;
        context.Response.Headers["X-Correlation-Id"] = correlationId;

        if (status == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(ex,
                "Unhandled exception. CorrelationId={CorrelationId} Path={Path}",
                correlationId, context.Request.Path);
        }
        else
        {
            _logger.LogWarning(
                "Handled {ExceptionType}: {Message} CorrelationId={CorrelationId}",
                ex.GetType().Name, ex.Message, correlationId);
        }

        context.Response.StatusCode  = status;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}