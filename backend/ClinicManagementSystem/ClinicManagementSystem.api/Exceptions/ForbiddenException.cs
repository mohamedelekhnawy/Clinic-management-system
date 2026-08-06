using ClinicManagementSystem.api.Exceptions.Base;

namespace ClinicManagementSystem.api.Exceptions;

/// <summary>
/// Exception thrown when user is authenticated but not authorized to access a resource (403)
/// </summary>
public class ForbiddenException : ClinicManagementException
{
    public ForbiddenException(string message = "You do not have permission to access this resource.")
        : base(message, StatusCodes.Status403Forbidden)
    {
    }

    public ForbiddenException(string message, Exception innerException)
        : base(message, StatusCodes.Status403Forbidden, innerException)
    {
    }
}
