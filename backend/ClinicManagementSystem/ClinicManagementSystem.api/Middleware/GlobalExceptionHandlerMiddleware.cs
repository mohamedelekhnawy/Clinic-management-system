using ClinicManagementSystem.api.Contracts.Common;
using ClinicManagementSystem.api.Exceptions.Base;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

namespace ClinicManagementSystem.api.Middleware;

/// <summary>
/// Global exception handler middleware that catches all unhandled exceptions
/// and returns RFC 7807 Problem Details
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, detail, errors) = exception switch
        {
            // Custom exceptions
            ClinicManagementException customEx => (
                customEx.StatusCode,
                GetTitle(customEx.StatusCode),
                customEx.Message,
                customEx.Errors != null ? ConvertToErrorsDictionary(customEx.Errors) : null
            ),

            // Entity Framework exceptions - specific first
            DbUpdateConcurrencyException => (
                StatusCodes.Status409Conflict,
                "Conflict",
                "The record you attempted to update was modified by another user. Please reload and try again.",
                null
            ),

            DbUpdateException dbEx => (
                StatusCodes.Status500InternalServerError,
                "Database Error",
                "A database error occurred while processing your request.",
                _env.IsDevelopment() ? ConvertToErrorsDictionary([dbEx.InnerException?.Message ?? dbEx.Message]) : null
            ),

            // Validation exceptions
            FluentValidation.ValidationException validationEx => (
                StatusCodes.Status400BadRequest,
                "Validation Error",
                "One or more validation errors occurred.",
                validationEx.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
            ),

            // Argument exceptions
            ArgumentNullException argNullEx => (
                StatusCodes.Status400BadRequest,
                "Bad Request",
                $"A required parameter was not provided: {argNullEx.ParamName}",
                null
            ),

            ArgumentException argEx => (
                StatusCodes.Status400BadRequest,
                "Bad Request",
                argEx.Message,
                null
            ),

            // Unauthorized access
            UnauthorizedAccessException => (
                StatusCodes.Status403Forbidden,
                "Forbidden",
                "You do not have permission to access this resource.",
                null
            ),

            // Operation cancelled (client disconnected)
            OperationCanceledException => (
                StatusCodes.Status499ClientClosedRequest,
                "Client Closed Request",
                "Request was cancelled.",
                null
            ),

            // Invalid operation
            InvalidOperationException invalidOpEx => (
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "An invalid operation was attempted.",
                _env.IsDevelopment() ? ConvertToErrorsDictionary([invalidOpEx.Message]) : null
            ),

            // Default case - all other exceptions
            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "An unexpected error occurred. Please try again later.",
                _env.IsDevelopment() ? ConvertToErrorsDictionary([exception.Message, exception.StackTrace ?? ""]) : null
            )
        };

        var problemDetails = new ProblemDetailsResponse
        {
            Type = GetProblemTypeUri(statusCode),
            Title = title,
            Status = statusCode,
            Detail = detail,
            Instance = context.Request.Path,
            TraceId = context.TraceIdentifier,
            Timestamp = DateTime.UtcNow,
            Errors = errors
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _env.IsDevelopment(),
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, options));
    }

    private static string GetProblemTypeUri(int statusCode) => statusCode switch
    {
        400 => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        401 => "https://tools.ietf.org/html/rfc7235#section-3.1",
        403 => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
        404 => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        409 => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
        422 => "https://tools.ietf.org/html/rfc4918#section-11.2",
        500 => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
        _ => "about:blank"
    };

    private static string GetTitle(int statusCode) => statusCode switch
    {
        400 => "Bad Request",
        401 => "Unauthorized",
        403 => "Forbidden",
        404 => "Not Found",
        409 => "Conflict",
        422 => "Unprocessable Entity",
        500 => "Internal Server Error",
        _ => "Error"
    };

    private static Dictionary<string, string[]> ConvertToErrorsDictionary(string[] errors)
    {
        return new Dictionary<string, string[]>
        {
            ["errors"] = errors
        };
    }
}
