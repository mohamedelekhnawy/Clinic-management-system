using ClinicManagementSystem.api.Contracts.Profile;

namespace ClinicManagementSystem.api.Services
{
    public class UserService(UserManager<ApplicationUser> userManager, IProfileService profileService) : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IProfileService _profileService = profileService;

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

        public async Task<Result<UserProfileResponse>> UpdateProfileAsync(string userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
        {
            // Get user with profile
            var user = await _userManager.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user is null)
                return Result.Failure<UserProfileResponse>(AuthErrors.UserNotFound);

            if (user.Profile is null)
                return Result.Failure<UserProfileResponse>(ProfileErrors.UserHasNoProfile);

            // Update profile entity
            var updatedProfile = new Models.Profile
            {
                FirstName_En = request.FirstName_En,
                FirstName_Ar = request.FirstName_Ar,
                LastName_En = request.LastName_En,
                LastName_Ar = request.LastName_Ar,
                Phone = request.Phone,
                Email = request.Email,
                IsActive = user.Profile.IsActive // Keep existing active status
            };

            var updateResult = await _profileService.UpdateAsync(user.Profile.Id, updatedProfile, cancellationToken);

            if (updateResult.IsFailure)
                return Result.Failure<UserProfileResponse>(updateResult.Error);

            // Fetch updated profile to return
            var response = await _userManager.Users
                .Where(x => x.Id == userId)
                .Select(u => new UserProfileResponse(
                    u.Id,
                    u.Email!,
                    u.IsEmailVerified,
                    u.EmailVerifiedAt,
                    u.Profile != null ? new ProfileResponse(
                        u.Profile.Id,
                        u.Profile.FirstName_En,
                        u.Profile.FirstName_Ar,
                        u.Profile.LastName_En,
                        u.Profile.LastName_Ar,
                        u.Profile.Phone,
                        u.Profile.Email,
                        u.Profile.IsActive,
                        u.Profile.CreatedOn,
                        u.Profile.UpdatedOn
                    ) : null
                ))
                .SingleOrDefaultAsync(cancellationToken);

            return response is null 
                ? Result.Failure<UserProfileResponse>(AuthErrors.UserNotFound)
                : Result.Success(response);
        }
    }
}
