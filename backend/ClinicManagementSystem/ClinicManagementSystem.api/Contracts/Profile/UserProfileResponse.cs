namespace ClinicManagementSystem.api.Contracts.Profile
{
    public record UserProfileResponse(
        string Id,
        string FirstName_EN,
        string? FirstName_AR,
        string LastName_EN,
        string? LastName_AR,
        string Email,
        bool IsEmailVerified,
        DateTime? EmailVerifiedAt
    );
}
