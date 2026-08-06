using ClinicManagementSystem.api.Exceptions.Base;

namespace ClinicManagementSystem.api.Exceptions;

/// <summary>
/// Exception thrown when a requested resource is not found (404)
/// </summary>
public class NotFoundException : ClinicManagementException
{
    public NotFoundException(string message)
        : base(message, StatusCodes.Status404NotFound)
    {
    }

    public NotFoundException(string resourceName, object key)
        : base($"{resourceName} with id '{key}' was not found.", StatusCodes.Status404NotFound)
    {
    }
}
