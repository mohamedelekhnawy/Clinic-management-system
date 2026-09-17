
using Hangfire;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using ClinicManagementSystem.api.Contracts.Profile;
using ClinicManagementSystem.api.Persistence;

namespace ClinicManagementSystem.api.Services
{
    public class AuthService(
        UserManager<ApplicationUser> userManager, 
        IJwtProvider jwtProvider,
        IEmailService emailService,
        IBackgroundJobClient backgroundJobClient,
        ApplicationDbContext context,
        ILogger<AuthService> logger) : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IJwtProvider _jwtProvider = jwtProvider;
        private readonly IEmailService _emailService = emailService;
        private readonly IBackgroundJobClient _backgroundJobClient = backgroundJobClient;
        private readonly ApplicationDbContext _context = context;
        private readonly ILogger<AuthService> _logger = logger;

        public async Task<Result<LoginResponse>> GetTokenAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            // Find user by email - RefreshTokens won't be loaded due to AutoInclude(false) in UserConfiguration
            var user = await _userManager.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

            if (user is null)
                return Result.Failure<LoginResponse>(AuthErrors.InvalidCredentials);

            // Password verification is intentionally slow (~100-300ms) for security (PBKDF2 hashing)
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
            if (!isPasswordValid)
                return Result.Failure<LoginResponse>(AuthErrors.InvalidCredentials);

            if (!user.IsEmailVerified)
            {
                _logger.LogWarning("Login attempt for unverified email: {Email}", email);
                return Result.Failure<LoginResponse>(AuthErrors.EmailNotVerified);
            }

            try
            {
                var (token, expiresIn) = _jwtProvider.GenerateToken(user);
                var refreshToken = _jwtProvider.GenerateRefreshToken();
                var refreshTokenExpiration = DateTime.UtcNow.AddDays(7);

                // Insert refresh token directly without loading the collection
                await _context.Database.ExecuteSqlRawAsync(
                    @"INSERT INTO RefreshTokens (UserId, Token, ExpiresOn, CreatedOn) 
                      VALUES ({0}, {1}, {2}, {3})",
                    user.Id, refreshToken, refreshTokenExpiration, DateTime.UtcNow);

                _logger.LogInformation("User {UserId} logged in successfully", user.Id);

                return Result.Success(new LoginResponse(user.Id, user.Email!, token, expiresIn, refreshToken, refreshTokenExpiration));
            }
            catch (DbUpdateException)
            {
                throw;
            }
        }

        public async Task<Result<RegisterResponse>> RegisterAsync(string firstNameEN, string? firstNameAR, string lastNameEN, string? lastNameAR, string phone, string email, string password, CancellationToken cancellationToken = default)
        {
            // Check if email already exists - RefreshTokens won't be loaded due to AutoInclude(false)
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser is not null)
                return Result.Failure<RegisterResponse>(AuthErrors.EmailAlreadyExists);

            // Check if phone already exists in Profiles
            var existingProfile = await _context.Profiles
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Phone == phone, cancellationToken);
            if (existingProfile is not null)
                return Result.Failure<RegisterResponse>(ProfileErrors.DuplicatePhone);

            // Check if email already exists in Profiles
            var existingProfileByEmail = await _context.Profiles
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Email == email, cancellationToken);
            if (existingProfileByEmail is not null)
                return Result.Failure<RegisterResponse>(ProfileErrors.DuplicateEmail);

            // Generate verification code upfront before user creation
            var verificationCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            var expirationTime = DateTime.UtcNow.AddMinutes(15);
            var currentTime = DateTime.UtcNow;

            // Create Profile first
            var profile = new Profile
            {
                FirstName_En = firstNameEN,
                FirstName_Ar = firstNameAR ?? string.Empty,
                LastName_En = lastNameEN,
                LastName_Ar = lastNameAR ?? string.Empty,
                Phone = phone,
                Email = email,
                IsActive = true
            };

            // Initialize user with all fields including verification data to avoid extra UpdateAsync
            var user = new ApplicationUser
            {
                Email = email,
                UserName = email,
                IsEmailVerified = false,
                EmailVerificationCode = verificationCode,
                EmailVerificationCodeExpiresAt = expirationTime,
                LastVerificationCodeSentAt = currentTime
            };

            try
            {
                // Start a transaction to ensure atomicity
                using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                
                try
                {
                    // Add profile and save to get its ID
                    await _context.Profiles.AddAsync(profile, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);

                    // Link the profile to the user
                    user.ProfileId = profile.Id;

                    // Password hashing happens here (~200-400ms, intentional for security)
                    var result = await _userManager.CreateAsync(user, password);
                    if (!result.Succeeded)
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        await transaction.RollbackAsync(cancellationToken);
                        return Result.Failure<RegisterResponse>(new Error("Auth.RegistrationFailed", errors, ErrorType.Validation));
                    }

                    // Update Profile with the ApplicationUserId after user creation
                    profile.ApplicationUserId = user.Id;
                    await _context.SaveChangesAsync(cancellationToken);

                    // Commit the transaction
                    await transaction.CommitAsync(cancellationToken);

                    // Enqueue verification email as Hangfire background job
                    _backgroundJobClient.Enqueue<IEmailService>(
                        emailService => emailService.SendVerificationCodeAsync(user.Email!, user.Email!, verificationCode, CancellationToken.None));
                    
                    _logger.LogInformation("User {UserId} registered successfully with Profile {ProfileId}. Verification email job enqueued for {Email}", 
                        user.Id, profile.Id, user.Email);

                    return Result.Success(new RegisterResponse(
                        user.Id,
                        user.Email!,
                        "Registration successful. Please check your email for verification code."
                    ));
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            }
            catch (DbUpdateException ex)
            {
                // Check for duplicate constraints
                if (ex.InnerException?.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) == true ||
                    ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true)
                {
                    if (ex.InnerException?.Message.Contains("Email", StringComparison.OrdinalIgnoreCase) == true)
                        return Result.Failure<RegisterResponse>(ProfileErrors.DuplicateEmail);

                    if (ex.InnerException?.Message.Contains("Phone", StringComparison.OrdinalIgnoreCase) == true)
                        return Result.Failure<RegisterResponse>(ProfileErrors.DuplicatePhone);
                    
                    return Result.Failure<RegisterResponse>(AuthErrors.EmailAlreadyExists);
                }
                
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

        public async Task<Result<RefreshTokenResponse>> RefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
        {
            var principal = _jwtProvider.ValidateToken(token);
            if (principal is null)
                return Result.Failure<RefreshTokenResponse>(AuthErrors.InvalidToken);

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier) 
                ?? principal.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
            
            if (string.IsNullOrEmpty(userId))
                return Result.Failure<RefreshTokenResponse>(AuthErrors.InvalidToken);

            // Load user with only the specific refresh token we need
            var user = await _userManager.Users
                .Include(u => u.RefreshTokens.Where(rt => rt.Token == refreshToken))
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user is null)
                return Result.Failure<RefreshTokenResponse>(AuthErrors.UserNotFound);

            var userRefreshToken = user.RefreshTokens.FirstOrDefault();
            if (userRefreshToken is null || !userRefreshToken.IsActive)
                return Result.Failure<RefreshTokenResponse>(AuthErrors.InvalidRefreshToken);

            try
            {
                // Revoke old token
                userRefreshToken.RevokedOn = DateTime.UtcNow;

                // Generate new tokens
                var (newToken, expiresIn) = _jwtProvider.GenerateToken(user);
                var newRefreshToken = _jwtProvider.GenerateRefreshToken();
                var newRefreshTokenExpiration = DateTime.UtcNow.AddDays(7);

                // Add new refresh token to user's collection
                user.RefreshTokens.Add(new RefreshToken
                {
                    Token = newRefreshToken,
                    ExpiresOn = newRefreshTokenExpiration,
                    CreatedOn = DateTime.UtcNow
                });

                await _userManager.UpdateAsync(user);

                var response = new RefreshTokenResponse(newToken, expiresIn, newRefreshToken, newRefreshTokenExpiration);
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
            // Query user with only the specific token we need
            var user = await _userManager.Users
                .Include(u => u.RefreshTokens.Where(rt => rt.Token == refreshToken))
                .FirstOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.Token == refreshToken), cancellationToken);

            if (user is null)
                return Result.Failure(AuthErrors.InvalidRefreshToken);

            var tokenToRevoke = user.RefreshTokens.FirstOrDefault();
            if (tokenToRevoke is null || !tokenToRevoke.IsActive)
                return Result.Failure(AuthErrors.InvalidRefreshToken);

            try
            {
                tokenToRevoke.RevokedOn = DateTime.UtcNow;
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

                // Enqueue verification email as a Hangfire background job
                _backgroundJobClient.Enqueue<IEmailService>(
                    emailService => emailService.SendVerificationCodeAsync(user.Email!, user.Email!, verificationCode, CancellationToken.None));
                _logger.LogInformation("Verification email job enqueued for {Email}, user {UserId}", user.Email, user.Id);

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
