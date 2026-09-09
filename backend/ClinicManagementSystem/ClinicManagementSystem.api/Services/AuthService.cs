
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;

namespace ClinicManagementSystem.api.Services
{
    public class AuthService(
        UserManager<ApplicationUser> userManager, 
        IJwtProvider jwtProvider,
        IEmailService emailService,
        ILogger<AuthService> logger) : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IJwtProvider _jwtProvider = jwtProvider;
        private readonly IEmailService _emailService = emailService;
        private readonly ILogger<AuthService> _logger = logger;

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

            // Check if email is verified
            if (!user.IsEmailVerified)
            {
                _logger.LogWarning("Login attempt for unverified email: {Email}", email);
                return Result.Failure<AuthResponse>(AuthErrors.EmailNotVerified);
            }

            try
            {
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

                _logger.LogInformation("User {UserId} logged in successfully", user.Id);

                var response = new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, token, expiresIn, refreshToken, refreshTokenExpiration);
                return Result.Success(response);
            }
            catch (DbUpdateException)
            {
                // Let database errors bubble up to global handler
                throw;
            }
        }

        public async Task<Result<VerificationResponse>> RegisterAsync(string firstName, string lastName, string email, string password, CancellationToken cancellationToken = default)
        {
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser is not null)
                return Result.Failure<VerificationResponse>(AuthErrors.EmailAlreadyExists);

            var user = new ApplicationUser
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                UserName = email,
                IsEmailVerified = false
            };

            try
            {
                var result = await _userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return Result.Failure<VerificationResponse>(new Error("Auth.RegistrationFailed", errors, ErrorType.Validation));
                }

                // Generate secure 6-digit OTP using RandomNumberGenerator
                var verificationCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
                var expirationTime = DateTime.UtcNow.AddMinutes(15);

                user.EmailVerificationCode = verificationCode;
                user.EmailVerificationCodeExpiresAt = expirationTime;
                user.LastVerificationCodeSentAt = DateTime.UtcNow;

                await _userManager.UpdateAsync(user);

                // Send verification email
                try
                {
                    await _emailService.SendVerificationCodeAsync(user.Email!, user.FirstName, verificationCode, cancellationToken);
                    _logger.LogInformation("Verification email sent to {Email} for user {UserId}", user.Email, user.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send verification email to {Email}", user.Email);
                    // Continue - user created but email failed. They can use resend.
                }

                var response = new VerificationResponse("Registration successful. Please check your email for verification code.");
                return Result.Success(response);
            }
            catch (DbUpdateException ex)
            {
                // Check for duplicate email constraint
                if (ex.InnerException?.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) == true ||
                    ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true)
                {
                    return Result.Failure<VerificationResponse>(AuthErrors.EmailAlreadyExists);
                }
                
                // Let other database errors bubble up to global handler
                throw;
            }
        }

        public async Task<Result<VerificationResponse>> VerifyEmailAsync(string email, string code, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
                return Result.Failure<VerificationResponse>(AuthErrors.UserNotFound);

            if (user.IsEmailVerified)
                return Result.Failure<VerificationResponse>(AuthErrors.EmailAlreadyVerified);

            if (string.IsNullOrEmpty(user.EmailVerificationCode))
                return Result.Failure<VerificationResponse>(AuthErrors.VerificationCodeNotFound);

            if (user.EmailVerificationCodeExpiresAt == null || DateTime.UtcNow > user.EmailVerificationCodeExpiresAt)
                return Result.Failure<VerificationResponse>(AuthErrors.VerificationCodeExpired);

            if (!user.EmailVerificationCode.Equals(code, StringComparison.Ordinal))
                return Result.Failure<VerificationResponse>(AuthErrors.InvalidVerificationCode);

            try
            {
                // Mark email as verified
                user.IsEmailVerified = true;
                user.EmailVerifiedAt = DateTime.UtcNow;
                user.EmailVerificationCode = null;
                user.EmailVerificationCodeExpiresAt = null;
                user.LastVerificationCodeSentAt = null;

                await _userManager.UpdateAsync(user);

                _logger.LogInformation("Email verified successfully for user {UserId}", user.Id);

                var response = new VerificationResponse("Email verified successfully. You can now login.");
                return Result.Success(response);
            }
            catch (DbUpdateException)
            {
                // Let database errors bubble up to global handler
                throw;
            }
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

            try
            {
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
            catch (DbUpdateException)
            {
                // Let database errors bubble up to global handler
                throw;
            }
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

            try
            {
                userRefreshToken.RevokedOn = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                return Result.Success();
            }
            catch (DbUpdateException)
            {
                // Let database errors bubble up to global handler
                throw;
            }
        }

        public async Task<Result<VerificationResponse>> ResendVerificationCodeAsync(string email, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
                return Result.Failure<VerificationResponse>(AuthErrors.UserNotFound);

            if (user.IsEmailVerified)
                return Result.Failure<VerificationResponse>(AuthErrors.EmailAlreadyVerified);

            // Mandatory rate limiting: 60-second cooldown
            if (user.LastVerificationCodeSentAt.HasValue)
            {
                var timeSinceLastSent = DateTime.UtcNow - user.LastVerificationCodeSentAt.Value;
                if (timeSinceLastSent.TotalSeconds < 60)
                {
                    _logger.LogWarning("Rate limit hit for resend verification code for {Email}. Last sent: {LastSent}", 
                        email, user.LastVerificationCodeSentAt);
                    return Result.Failure<VerificationResponse>(AuthErrors.TooManyVerificationAttempts);
                }
            }

            try
            {
                // Generate new secure 6-digit OTP using RandomNumberGenerator
                var verificationCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
                var expirationTime = DateTime.UtcNow.AddMinutes(15);

                user.EmailVerificationCode = verificationCode;
                user.EmailVerificationCodeExpiresAt = expirationTime;
                user.LastVerificationCodeSentAt = DateTime.UtcNow;

                await _userManager.UpdateAsync(user);

                // Send verification email
                try
                {
                    await _emailService.SendVerificationCodeAsync(user.Email!, user.FirstName, verificationCode, cancellationToken);
                    _logger.LogInformation("Verification code resent to {Email} for user {UserId}", user.Email, user.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to resend verification email to {Email}", user.Email);
                    throw; // Email sending failure should fail the operation
                }

                var response = new VerificationResponse("Verification code sent. Please check your email.");
                return Result.Success(response);
            }
            catch (DbUpdateException)
            {
                // Let database errors bubble up to global handler
                throw;
            }
        }
    }
}
