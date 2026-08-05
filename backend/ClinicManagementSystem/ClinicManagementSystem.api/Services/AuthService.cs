
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClinicManagementSystem.api.Services
{
    public class AuthService(UserManager<ApplicationUser> userManager, IJwtProvider jwtProvider) : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IJwtProvider _jwtProvider = jwtProvider;

        public async Task<Result<AuthResponse>> GetTokenAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

            if (user is null)
                return Result.Failure<AuthResponse>(AuthErrors.InvalidCredentials);

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
            if (!isPasswordValid)
                return Result.Failure<AuthResponse>(AuthErrors.InvalidCredentials);

            var (token, expiresIn) = _jwtProvider.GenerateToken(user);
            var refreshToken = _jwtProvider.GenerateRefreshToken();
            var refreshTokenExpiration = DateTime.UtcNow.AddDays(7);

            user.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                ExpiresOn = refreshTokenExpiration,
                CreatedOn = DateTime.UtcNow
            });

            await _userManager.UpdateAsync(user);

            var response = new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, token, expiresIn, refreshToken, refreshTokenExpiration);
            return Result.Success(response);
        }

        public async Task<Result<AuthResponse>> RegisterAsync(string firstName, string lastName, string email, string password, CancellationToken cancellationToken = default)
        {
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser is not null)
                return Result.Failure<AuthResponse>(AuthErrors.EmailAlreadyExists);

            var user = new ApplicationUser
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                UserName = email
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                return Result.Failure<AuthResponse>(AuthErrors.RegistrationFailed);

            var (token, expiresIn) = _jwtProvider.GenerateToken(user);
            var refreshToken = _jwtProvider.GenerateRefreshToken();
            var refreshTokenExpiration = DateTime.UtcNow.AddDays(7);

            user.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                ExpiresOn = refreshTokenExpiration,
                CreatedOn = DateTime.UtcNow
            });

            await _userManager.UpdateAsync(user);

            var response = new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, token, expiresIn, refreshToken, refreshTokenExpiration);
            return Result.Success(response);
        }

        public async Task<Result<AuthResponse>> RefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
        {
            var principal = _jwtProvider.ValidateToken(token);
            if (principal is null)
                return Result.Failure<AuthResponse>(AuthErrors.InvalidToken);

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier) 
                ?? principal.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
            
            if (string.IsNullOrEmpty(userId))
                return Result.Failure<AuthResponse>(AuthErrors.InvalidToken);

            var user = await _userManager.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user is null)
                return Result.Failure<AuthResponse>(AuthErrors.UserNotFound);

            var userRefreshToken = user.RefreshTokens.FirstOrDefault(rt => rt.Token == refreshToken);
            if (userRefreshToken is null || !userRefreshToken.IsActive)
                return Result.Failure<AuthResponse>(AuthErrors.InvalidRefreshToken);

            userRefreshToken.RevokedOn = DateTime.UtcNow;

            var (newToken, expiresIn) = _jwtProvider.GenerateToken(user);
            var newRefreshToken = _jwtProvider.GenerateRefreshToken();
            var newRefreshTokenExpiration = DateTime.UtcNow.AddDays(7);

            user.RefreshTokens.Add(new RefreshToken
            {
                Token = newRefreshToken,
                ExpiresOn = newRefreshTokenExpiration,
                CreatedOn = DateTime.UtcNow
            });

            await _userManager.UpdateAsync(user);

            var response = new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, newToken, expiresIn, newRefreshToken, newRefreshTokenExpiration);
            return Result.Success(response);
        }

        public async Task<Result> RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.Token == refreshToken), cancellationToken);

            if (user is null)
                return Result.Failure(AuthErrors.InvalidRefreshToken);

            var userRefreshToken = user.RefreshTokens.FirstOrDefault(rt => rt.Token == refreshToken);
            if (userRefreshToken is null || !userRefreshToken.IsActive)
                return Result.Failure(AuthErrors.InvalidRefreshToken);

            userRefreshToken.RevokedOn = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            return Result.Success();
        }
    }
}
