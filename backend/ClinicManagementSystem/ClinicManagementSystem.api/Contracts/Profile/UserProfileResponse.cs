using ClinicManagementSystem.api.Contracts.Profile;

namespace ClinicManagementSystem.api.Contracts.Profile
{
    public record UserProfileResponse(
        string Id,
        string Email,
        bool IsEmailVerified,
        DateTime? EmailVerifiedAt,
        ProfileResponse? Profile
    );
}
