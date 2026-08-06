using ClinicManagementSystem.api.Exceptions.Base;

namespace ClinicManagementSystem.api.Exceptions;

/// <summary>
/// Exception thrown when a bad request is made (400)
/// </summary>
public class BadRequestException : ClinicManagementException
{
    public BadRequestException(string message)
        : base(message, StatusCodes.Status400BadRequest)
    {
    }

    public BadRequestException(string message, Exception innerException)
        : base(message, StatusCodes.Status400BadRequest, innerException)
    {
    }
}
