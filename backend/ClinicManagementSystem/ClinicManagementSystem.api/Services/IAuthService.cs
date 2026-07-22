using ClinicManagementSystem.api.Contracts.Authentication;

namespace ClinicManagementSystem.api.Services
{
    public interface IAuthService
    {
        Task<AuthResponse?> GetTokenAsync(string email, string password, CancellationToken cancellationToken = default);
        Task<AuthResponse?> RegisterAsync(string firstName, string lastName, string email, string password, CancellationToken cancellationToken = default);
    }
}
