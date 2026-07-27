namespace ClinicManagementSystem.api.Services
{
    public interface IAuthService
    {
        Task<AuthResponse?> GetTokenAsync(string email, string password, CancellationToken cancellationToken = default);
        Task<AuthResponse?> RegisterAsync(string firstName, string lastName, string email, string password, CancellationToken cancellationToken = default);
        Task<AuthResponse?> RefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default);
        Task<bool> RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    }
}
