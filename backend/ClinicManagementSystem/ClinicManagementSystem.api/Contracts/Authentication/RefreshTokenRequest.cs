namespace ClinicManagementSystem.api.Contracts.Authentication
{
    public record RefreshTokenRequest(
        string Token,
        string RefreshToken
    );
}
