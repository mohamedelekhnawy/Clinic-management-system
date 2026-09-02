namespace ClinicManagementSystem.api.Abstractions;

public static class PatientErrors
{
    public static readonly Error NotFound = new(
        "Patient.NotFound",
        "Patient not found",
        ErrorType.NotFound);

    public static readonly Error DuplicatePhone = new(
        "Patient.DuplicatePhone",
        "A patient with this phone number already exists",
        ErrorType.Conflict);

    public static readonly Error DuplicateEmail = new(
        "Patient.DuplicateEmail",
        "A patient with this email already exists",
        ErrorType.Conflict);

    public static readonly Error InvalidDateOfBirth = new(
        "Patient.InvalidDateOfBirth",
        "Date of birth cannot be in the future",
        ErrorType.Validation);

    public static readonly Error HasDependentRecords = new(
        "Patient.HasDependentRecords",
        "Cannot delete patient because they have associated appointments or medical records",
        ErrorType.Conflict);
}
