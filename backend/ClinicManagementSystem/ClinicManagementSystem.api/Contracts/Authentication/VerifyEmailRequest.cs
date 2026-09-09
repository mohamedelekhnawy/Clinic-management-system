namespace ClinicManagementSystem.api.Contracts.Authentication
{
    public record VerifyEmailRequest(
        string Email,
        string Code
    );
}
