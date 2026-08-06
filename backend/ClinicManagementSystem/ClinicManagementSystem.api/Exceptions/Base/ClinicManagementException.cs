namespace ClinicManagementSystem.api.Exceptions.Base;

/// <summary>
/// Base exception class for all custom exceptions in the Clinic Management System
/// </summary>
public abstract class ClinicManagementException : Exception
{
    public int StatusCode { get; }
    public string[]? Errors { get; }

    protected ClinicManagementException(string message, int statusCode, string[]? errors = null)
        : base(message)
    {
        StatusCode = statusCode;
        Errors = errors;
    }

    protected ClinicManagementException(string message, int statusCode, Exception innerException, string[]? errors = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        Errors = errors;
    }
}
