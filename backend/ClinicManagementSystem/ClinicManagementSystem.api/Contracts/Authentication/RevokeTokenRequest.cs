namespace ClinicManagementSystem.api.Contracts.Authentication
{
    public record RevokeTokenRequest(
        string RefreshToken
    );
}
