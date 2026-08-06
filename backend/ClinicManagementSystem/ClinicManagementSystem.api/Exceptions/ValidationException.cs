using ClinicManagementSystem.api.Exceptions.Base;

namespace ClinicManagementSystem.api.Exceptions;

/// <summary>
/// Exception thrown when validation fails (400)
/// </summary>
public class ValidationException : ClinicManagementException
{
    public ValidationException(string message)
        : base(message, StatusCodes.Status400BadRequest)
    {
    }

    public ValidationException(string message, string[] errors)
        : base(message, StatusCodes.Status400BadRequest, errors)
    {
    }

    public ValidationException(string[] errors)
        : base("One or more validation errors occurred.", StatusCodes.Status400BadRequest, errors)
    {
    }
}
