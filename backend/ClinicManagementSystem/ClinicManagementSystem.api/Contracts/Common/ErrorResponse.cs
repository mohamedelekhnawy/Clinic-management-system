namespace ClinicManagementSystem.api.Contracts.Common;

/// <summary>
/// RFC 7807 Problem Details for HTTP APIs
/// https://tools.ietf.org/html/rfc7807
/// </summary>
public sealed record ProblemDetailsResponse
{
    /// <summary>
    /// A URI reference that identifies the problem type
    /// </summary>
    public string Type { get; init; } = "about:blank";

    /// <summary>
    /// A short, human-readable summary of the problem type
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// The HTTP status code
    /// </summary>
    public int Status { get; init; }

    /// <summary>
    /// A human-readable explanation specific to this occurrence of the problem
    /// </summary>
    public string Detail { get; init; } = string.Empty;

    /// <summary>
    /// A URI reference that identifies the specific occurrence of the problem
    /// </summary>
    public string? Instance { get; init; }

    /// <summary>
    /// Trace identifier for debugging
    /// </summary>
    public string? TraceId { get; init; }

    /// <summary>
    /// Timestamp when the error occurred
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Additional validation errors (optional)
    /// </summary>
    public Dictionary<string, string[]>? Errors { get; init; }
}
