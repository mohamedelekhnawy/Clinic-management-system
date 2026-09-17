namespace ClinicManagementSystem.api.Abstractions;

public static class ProfileErrors
{
    public static readonly Error NotFound = new(
        "Profile.NotFound",
        "Profile not found",
        ErrorType.NotFound);

    public static readonly Error DuplicateEmail = new(
        "Profile.DuplicateEmail",
        "A profile with this email already exists",
        ErrorType.Conflict);

    public static readonly Error DuplicatePhone = new(
        "Profile.DuplicatePhone",
        "A profile with this phone number already exists",
        ErrorType.Conflict);

    public static readonly Error HasDependentRecords = new(
        "Profile.HasDependentRecords",
        "Cannot delete profile because it has dependent records",
        ErrorType.Conflict);

    public static readonly Error UserHasNoProfile = new(
        "Profile.UserHasNoProfile",
        "User does not have a profile",
        ErrorType.NotFound);
}
