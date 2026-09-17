namespace ClinicManagementSystem.api.Contracts.Authentication
{
    public record RegisterResponse(
        string UserId,
        string Email,
        string Message
    );
}
