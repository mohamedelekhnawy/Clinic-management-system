namespace ClinicManagementSystem.api.Abstractions;

public static class AuthErrors
{
    public static readonly Error InvalidCredentials = new(
        "Auth.InvalidCredentials",
        "Invalid email or password",
        ErrorType.Validation);

    public static readonly Error EmailAlreadyExists = new(
        "Auth.EmailAlreadyExists",
        "Email is already registered",
        ErrorType.Conflict);

    public static readonly Error RegistrationFailed = new(
        "Auth.RegistrationFailed",
        "User registration failed",
        ErrorType.Failure);

    public static readonly Error InvalidToken = new(
        "Auth.InvalidToken",
        "Invalid or expired token",
        ErrorType.Validation);

    public static readonly Error InvalidRefreshToken = new(
        "Auth.InvalidRefreshToken",
        "Invalid or expired refresh token",
        ErrorType.Validation);

    public static readonly Error UserNotFound = new(
        "Auth.UserNotFound",
        "User not found",
        ErrorType.NotFound);
}
