namespace ClinicManagementSystem.api.Abstractions;

public static class DoctorErrors
{
    public static readonly Error NotFound = new(
        "Doctor.NotFound",
        "Doctor not found",
        ErrorType.NotFound);

    public static readonly Error ClinicNotFound = new(
        "Doctor.ClinicNotFound",
        "The specified clinic does not exist",
        ErrorType.NotFound);

    public static readonly Error DuplicateEmail = new(
        "Doctor.DuplicateEmail",
        "A doctor with this email already exists",
        ErrorType.Conflict);

    public static readonly Error DuplicatePhone = new(
        "Doctor.DuplicatePhone",
        "A doctor with this phone number already exists",
        ErrorType.Conflict);

    public static readonly Error HasDependentRecords = new(
        "Doctor.HasDependentRecords",
        "Cannot delete doctor because they have associated appointments or records",
        ErrorType.Conflict);
}
