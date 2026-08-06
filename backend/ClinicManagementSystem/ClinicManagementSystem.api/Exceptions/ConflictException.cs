using ClinicManagementSystem.api.Exceptions.Base;

namespace ClinicManagementSystem.api.Exceptions;

/// <summary>
/// Exception thrown when there is a conflict with the current state (409)
/// Example: Duplicate entry, constraint violation
/// </summary>
public class ConflictException : ClinicManagementException
{
    public ConflictException(string message)
        : base(message, StatusCodes.Status409Conflict)
    {
    }

    public ConflictException(string message, Exception innerException)
        : base(message, StatusCodes.Status409Conflict, innerException)
    {
    }
}
