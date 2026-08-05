namespace ClinicManagementSystem.api.Abstractions;

public static class ClinicErrors
{
    public static readonly Error NotFound = new(
        "Clinic.NotFound",
        "Clinic not found",
        ErrorType.NotFound);

    public static readonly Error AlreadyExists = new(
        "Clinic.AlreadyExists",
        "Clinic already exists",
        ErrorType.Conflict);
}
