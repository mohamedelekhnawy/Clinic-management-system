using ClinicManagementSystem.api.Exceptions.Base;

namespace ClinicManagementSystem.api.Exceptions;

/// <summary>
/// Exception thrown when authentication is required but not provided or invalid (401)
/// </summary>
public class UnauthorizedException : ClinicManagementException
{
    public UnauthorizedException(string message = "You are not authenticated. Please login.")
        : base(message, StatusCodes.Status401Unauthorized)
    {
    }

    public UnauthorizedException(string message, Exception innerException)
        : base(message, StatusCodes.Status401Unauthorized, innerException)
    {
    }
}
