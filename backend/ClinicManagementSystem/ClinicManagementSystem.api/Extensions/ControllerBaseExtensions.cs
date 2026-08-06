using ClinicManagementSystem.api.Contracts.Common;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagementSystem.api.Extensions;

/// <summary>
/// Extension methods for ControllerBase to return RFC 7807 Problem Details
/// </summary>
public static class ControllerBaseExtensions
{
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

    public static IActionResult Problem(
        this ControllerBase controller,
        Error error,
        int statusCode)
    {
        var problemDetails = new ProblemDetailsResponse
        {
            Type = GetProblemTypeUri(statusCode),
            Title = GetTitle(statusCode),
            Status = statusCode,
            Detail = error.Message,
            Instance = controller.HttpContext.Request.Path,
            TraceId = controller.HttpContext.TraceIdentifier,
            Timestamp = DateTime.UtcNow
        };

        return new ObjectResult(problemDetails)
        {
            StatusCode = statusCode
        };
    }

    public static IActionResult Problem(
        this ControllerBase controller,
        string detail,
        int statusCode)
    {
        var problemDetails = new ProblemDetailsResponse
        {
            Type = GetProblemTypeUri(statusCode),
            Title = GetTitle(statusCode),
            Status = statusCode,
            Detail = detail,
            Instance = controller.HttpContext.Request.Path,
            TraceId = controller.HttpContext.TraceIdentifier,
            Timestamp = DateTime.UtcNow
        };

        return new ObjectResult(problemDetails)
        {
            StatusCode = statusCode
        };
    }

    public static IActionResult ValidationProblem(
        this ControllerBase controller,
        Dictionary<string, string[]> errors)
    {
        var problemDetails = new ProblemDetailsResponse
        {
            Type = GetProblemTypeUri(400),
            Title = "One or more validation errors occurred",
            Status = 400,
            Detail = "See the errors property for details",
            Instance = controller.HttpContext.Request.Path,
            TraceId = controller.HttpContext.TraceIdentifier,
            Timestamp = DateTime.UtcNow,
            Errors = errors
        };

        return new ObjectResult(problemDetails)
        {
            StatusCode = 400
        };
    }
}
