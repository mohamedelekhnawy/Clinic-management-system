namespace ClinicManagementSystem.api.Contracts.Profile
{
    public record UserProfileResponse(
        string Id,
        string FirstName,
        string LastName,
        string Email,
        bool IsEmailVerified,
        DateTime? EmailVerifiedAt
    );
}
