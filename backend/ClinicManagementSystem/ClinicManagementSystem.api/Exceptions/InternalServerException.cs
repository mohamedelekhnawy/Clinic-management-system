using ClinicManagementSystem.api.Exceptions.Base;

namespace ClinicManagementSystem.api.Exceptions;

/// <summary>
/// Exception thrown when an internal server error occurs (500)
/// </summary>
public class InternalServerException : ClinicManagementException
{
    public InternalServerException(string message = "An internal server error occurred.")
        : base(message, StatusCodes.Status500InternalServerError)
    {
    }

    public InternalServerException(string message, Exception innerException)
        : base(message, StatusCodes.Status500InternalServerError, innerException)
    {
    }
}
