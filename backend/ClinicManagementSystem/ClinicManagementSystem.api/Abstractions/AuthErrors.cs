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

    // Email Verification Errors
    public static readonly Error EmailNotVerified = new(
        "Auth.EmailNotVerified",
        "Email address is not verified. Please check your email for verification code.",
        ErrorType.Validation);

    public static readonly Error InvalidVerificationCode = new(
        "Auth.InvalidVerificationCode",
        "Invalid verification code",
        ErrorType.Validation);

    public static readonly Error VerificationCodeExpired = new(
        "Auth.VerificationCodeExpired",
        "Verification code has expired. Please request a new one.",
        ErrorType.Validation);

    public static readonly Error VerificationCodeNotFound = new(
        "Auth.VerificationCodeNotFound",
        "No verification code found. Please request a new verification code.",
        ErrorType.NotFound);

    public static readonly Error TooManyVerificationAttempts = new(
        "Auth.TooManyVerificationAttempts",
        "Too many requests. Please wait before requesting another verification code.",
        ErrorType.Validation);

    public static readonly Error EmailAlreadyVerified = new(
        "Auth.EmailAlreadyVerified",
        "Email is already verified",
        ErrorType.Validation);
}
