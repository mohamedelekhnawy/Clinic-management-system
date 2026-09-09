using ClinicManagementSystem.api.Contracts.Profile;

namespace ClinicManagementSystem.api.Services
{
    public class UserService(UserManager<ApplicationUser> userManager) : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        public async Task<Result<UserProfileResponse>> GetProfileAsync(string userId, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.Users
                .Where(x => x.Id == userId)
                .ProjectToType<UserProfileResponse>()
                .SingleOrDefaultAsync(cancellationToken);

            if (user is null)
                return Result.Failure<UserProfileResponse>(AuthErrors.UserNotFound);

            return Result.Success(user);
        }
    }
}
