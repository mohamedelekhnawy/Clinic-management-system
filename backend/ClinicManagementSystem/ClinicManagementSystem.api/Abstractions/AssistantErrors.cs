namespace ClinicManagementSystem.api.Abstractions;

public static class AssistantErrors
{
    public static readonly Error NotFound = new(
        "Assistant.NotFound",
        "Assistant not found",
        ErrorType.NotFound);

    public static readonly Error ClinicNotFound = new(
        "Assistant.ClinicNotFound",
        "The specified clinic does not exist",
        ErrorType.NotFound);

    public static readonly Error DuplicateEmail = new(
        "Assistant.DuplicateEmail",
        "An assistant with this email already exists",
        ErrorType.Conflict);

    public static readonly Error DuplicatePhone = new(
        "Assistant.DuplicatePhone",
        "An assistant with this phone number already exists",
        ErrorType.Conflict);

    public static readonly Error HasDependentRecords = new(
        "Assistant.HasDependentRecords",
        "Cannot delete assistant because they have associated records",
        ErrorType.Conflict);
}
