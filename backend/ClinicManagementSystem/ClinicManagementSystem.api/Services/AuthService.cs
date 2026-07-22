using ClinicManagementSystem.api.Authentication;
using ClinicManagementSystem.api.Contracts.Authentication;

namespace ClinicManagementSystem.api.Services
{
    public class AuthService(UserManager<ApplicationUser> userManager, IJwtProvider jwtProvider) : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IJwtProvider _jwtProvider = jwtProvider;

        public async Task<AuthResponse?> GetTokenAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
                return null;

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
            if (!isPasswordValid)
                return null;

            var (token, expiresIn) = _jwtProvider.GenerateToken(user);

            return new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, token, expiresIn);
        }

        public async Task<AuthResponse?> RegisterAsync(string firstName, string lastName, string email, string password, CancellationToken cancellationToken = default)
        {
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser is not null)
                return null;

            var user = new ApplicationUser
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                UserName = email
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                return null;

            var (token, expiresIn) = _jwtProvider.GenerateToken(user);

            return new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, token, expiresIn);
        }
    }
}
