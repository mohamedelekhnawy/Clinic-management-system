using ClinicManagementSystem.api.Abstractions;

namespace ClinicManagementSystem.api.Services
{
    public interface IAuthService
    {
        Task<Result<AuthResponse>> GetTokenAsync(string email, string password, CancellationToken cancellationToken = default);
        Task<Result<VerificationResponse>> RegisterAsync(string firstName, string lastName, string email, string password, CancellationToken cancellationToken = default);
        Task<Result<AuthResponse>> RefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default);
        Task<Result> RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
        Task<Result<VerificationResponse>> VerifyEmailAsync(string email, string code, CancellationToken cancellationToken = default);
        Task<Result<VerificationResponse>> ResendVerificationCodeAsync(string email, CancellationToken cancellationToken = default);
    }
}
