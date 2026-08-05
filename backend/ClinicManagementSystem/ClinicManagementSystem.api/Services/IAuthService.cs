using ClinicManagementSystem.api.Abstractions;

namespace ClinicManagementSystem.api.Services
{
    public interface IAuthService
    {
        Task<Result<AuthResponse>> GetTokenAsync(string email, string password, CancellationToken cancellationToken = default);
        Task<Result<AuthResponse>> RegisterAsync(string firstName, string lastName, string email, string password, CancellationToken cancellationToken = default);
        Task<Result<AuthResponse>> RefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default);
        Task<Result> RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    }
}
