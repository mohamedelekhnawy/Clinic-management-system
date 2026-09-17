namespace ClinicManagementSystem.api.Models
{
    public sealed class ApplicationUser : IdentityUser
    {
        public int? ProfileId { get; set; }
        
        // Navigation properties
        public Profile? Profile { get; set; }
        public List<RefreshToken> RefreshTokens { get; set; } = new();

        // Email Verification Properties
        public string? EmailVerificationCode { get; set; }
        public DateTime? EmailVerificationCodeExpiresAt { get; set; }
        public bool IsEmailVerified { get; set; } = false;
        public DateTime? EmailVerifiedAt { get; set; }
        public DateTime? LastVerificationCodeSentAt { get; set; }
    }
}
