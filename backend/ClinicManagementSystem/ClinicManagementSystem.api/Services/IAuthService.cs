using ClinicManagementSystem.api.Abstractions;

namespace ClinicManagementSystem.api.Services
{
    public interface IAuthService
    {
        Task<Result<LoginResponse>> GetTokenAsync(string email, string password, CancellationToken cancellationToken = default);
        Task<Result<RegisterResponse>> RegisterAsync(string firstNameEN, string? firstNameAR, string lastNameEN, string? lastNameAR, string phone, string email, string password, CancellationToken cancellationToken = default);
        Task<Result<RefreshTokenResponse>> RefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default);
        Task<Result> RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
        Task<Result<VerificationResponse>> VerifyEmailAsync(string email, string code, CancellationToken cancellationToken = default);
        Task<Result<VerificationResponse>> ResendVerificationCodeAsync(string email, CancellationToken cancellationToken = default);
    }
}
