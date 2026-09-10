namespace ClinicManagementSystem.api.Contracts.Authentication
{
    public record AuthResponse(
        string Id,
        string? Email,
        string FirstName_EN,
        string? FirstName_AR,
        string LastName_EN,
        string? LastName_AR,
        string Token,
        int ExpiresIn,
        string RefreshToken,
        DateTime RefreshTokenExpiration
    );

}
