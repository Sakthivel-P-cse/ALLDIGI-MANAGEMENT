using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace SnipeIt.Api.Common;

/// <summary>
/// Catches unhandled exceptions from the pipeline and returns a consistent RFC-7807
/// ProblemDetails payload, so clients always receive structured JSON errors rather than
/// raw stack traces. Known domain exceptions map to specific status codes.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _env = env ?? throw new ArgumentNullException(nameof(env));
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
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            InvalidOperationException => (StatusCodes.Status400BadRequest, "Invalid operation"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid argument"),
            UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "Forbidden"),
            Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException =>
                (StatusCodes.Status409Conflict, "The record was modified by another user"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

        if (status >= 500)
        {
            _logger.LogError(ex, "Unhandled exception on {Method} {Path}",
                context.Request.Method, context.Request.Path);
        }
        else
        {
            _logger.LogWarning(ex, "Handled {Status} on {Method} {Path}",
                status, context.Request.Method, context.Request.Path);
        }

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = (status >= 500 && !_env.IsDevelopment()) ? null : ex.Message,
            Instance = context.Request.Path
        };

        context.Response.Clear();
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, options));
    }
}
