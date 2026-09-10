namespace ClinicManagementSystem.api.Models
{
    public sealed class ApplicationUser : IdentityUser
    {
        public string FirstName_EN { get; set; } = string.Empty;
        public string? FirstName_AR { get; set; }
        public string LastName_EN { get; set; } = string.Empty;
        public string? LastName_AR { get; set; }
        
        // Backward compatibility properties - use English by default
        public string FirstName 
        { 
            get => FirstName_EN; 
            set => FirstName_EN = value; 
        }
        
        public string LastName 
        { 
            get => LastName_EN; 
            set => LastName_EN = value; 
        }
        
        public List<RefreshToken> RefreshTokens { get; set; } = new();

        // Email Verification Properties
        public string? EmailVerificationCode { get; set; }
        public DateTime? EmailVerificationCodeExpiresAt { get; set; }
        public bool IsEmailVerified { get; set; } = false;
        public DateTime? EmailVerifiedAt { get; set; }
        public DateTime? LastVerificationCodeSentAt { get; set; }
    }
}
