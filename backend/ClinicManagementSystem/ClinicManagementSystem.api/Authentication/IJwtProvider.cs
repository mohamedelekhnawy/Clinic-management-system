using System.Security.Claims;

namespace ClinicManagementSystem.api.Authentication
{
    public interface IJwtProvider
    {
        (string token,int expiresIn) GenerateToken(ApplicationUser user);
        string GenerateRefreshToken();
        ClaimsPrincipal? ValidateToken(string token);
    }
}
