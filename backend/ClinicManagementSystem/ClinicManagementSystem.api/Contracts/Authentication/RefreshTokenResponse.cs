namespace ClinicManagementSystem.api.Contracts.Authentication
{
    public record RefreshTokenResponse(
        string Token,
        int ExpiresIn,
        string RefreshToken,
        DateTime RefreshTokenExpiration
    );
}
