namespace ClinicManagementSystem.api.Abstractions;

public static class DoctorErrors
{
    public static readonly Error NotFound = new(
        "Doctor.NotFound",
        "Doctor not found",
        ErrorType.NotFound);

    public static readonly Error AlreadyExists = new(
        "Doctor.AlreadyExists",
        "Doctor already exists",
        ErrorType.Conflict);
}
