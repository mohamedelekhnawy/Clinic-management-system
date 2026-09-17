using ClinicManagementSystem.api.Abstractions;
using ClinicManagementSystem.api.Contracts.Profile;

namespace ClinicManagementSystem.api.Services
{
    public interface IUserService
    {
        Task<Result<UserProfileResponse>> GetProfileAsync(string userId, CancellationToken cancellationToken = default);
        Task<Result<UserProfileResponse>> UpdateProfileAsync(string userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);
    }
}
