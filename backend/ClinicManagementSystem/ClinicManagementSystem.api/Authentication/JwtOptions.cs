using System.ComponentModel.DataAnnotations;

namespace ClinicManagementSystem.api.Authentication
{
    public class JwtOptions
    {
        public static string SectionName = "Jwt";

        [Required(ErrorMessage = "JWT Key is required")]
        [MinLength(32, ErrorMessage = "JWT Key must be at least 32 characters long")]
        public string Key { get; init; } = string.Empty;

        [Required(ErrorMessage = "JWT Issuer is required")]
        public string Issuer { get; init; } = string.Empty;

        [Required(ErrorMessage = "JWT Audience is required")]
        public string Audience { get; init; } = string.Empty;

        [Range(1, 10080, ErrorMessage = "ExpirationInMinutes must be between 1 and 10080 (7 days)")]
        public int ExpirationInMinutes { get; init; }
    }
}
