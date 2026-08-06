namespace ClinicManagementSystem.api.Abstractions;

public static class ClinicErrors
{
    public static readonly Error NotFound = new(
        "Clinic.NotFound",
        "Clinic not found",
        ErrorType.NotFound);

    public static readonly Error DuplicateName = new(
        "Clinic.DuplicateName",
        "A clinic with this name already exists",
        ErrorType.Conflict);

    public static readonly Error HasDependentDoctors = new(
        "Clinic.HasDependentDoctors",
        "Cannot delete clinic because it has associated doctors",
        ErrorType.Conflict);
}
