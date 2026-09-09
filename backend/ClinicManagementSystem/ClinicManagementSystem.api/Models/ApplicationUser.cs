namespace ClinicManagementSystem.api.Models
{
    public sealed class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public List<RefreshToken> RefreshTokens { get; set; } = new();

        // Email Verification Properties
        public string? EmailVerificationCode { get; set; }
        public DateTime? EmailVerificationCodeExpiresAt { get; set; }
        public bool IsEmailVerified { get; set; } = false;
        public DateTime? EmailVerifiedAt { get; set; }
        public DateTime? LastVerificationCodeSentAt { get; set; }
    }
}
