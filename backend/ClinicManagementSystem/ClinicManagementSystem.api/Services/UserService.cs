using ClinicManagementSystem.api.Contracts.Profile;

namespace ClinicManagementSystem.api.Services
{
    public class UserService(UserManager<ApplicationUser> userManager) : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        public async Task<Result<UserProfileResponse>> GetProfileAsync(string userId, CancellationToken cancellationToken = default)
        {
            // Use projection to reduce database load
            var response = await _userManager.Users
                .Where(x => x.Id == userId)
                .Select(user => new UserProfileResponse(
                    user.Id,
                    user.Email!,
                    user.IsEmailVerified,
                    user.EmailVerifiedAt,
                    user.Profile != null ? new ProfileResponse(
                        user.Profile.Id,
                        user.Profile.FirstName_En,
                        user.Profile.FirstName_Ar,
                        user.Profile.LastName_En,
                        user.Profile.LastName_Ar,
                        user.Profile.Phone,
                        user.Profile.Email,
                        user.Profile.IsActive,
                        user.Profile.CreatedOn,
                        user.Profile.UpdatedOn
                    ) : null
                ))
                .SingleOrDefaultAsync(cancellationToken);

            if (response is null)
                return Result.Failure<UserProfileResponse>(AuthErrors.UserNotFound);

            return Result.Success(response);
        }
    }
}
