namespace ClinicManagementSystem.api.Contracts.Authentication
{
    public record LoginResponse(
        string Id,
        string Email,
        string Token,
        int ExpiresIn,
        string RefreshToken,
        DateTime RefreshTokenExpiration
    );
}
