using ClinicManagementSystem.api.Extensions;

namespace ClinicManagementSystem.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
        {
            var result = await _authService.GetTokenAsync(request.Email, request.Password, cancellationToken);

            return result.IsSuccess
                ? Ok(result.Value)
                : this.Problem(result.Error, StatusCodes.Status400BadRequest);
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
        {
            var result = await _authService.RegisterAsync(request.FirstName_EN, request.FirstName_AR, request.LastName_EN, request.LastName_AR, request.Email, request.Password, cancellationToken);

            return result.IsSuccess
                ? Ok(result.Value)
                : this.Problem(result.Error, StatusCodes.Status400BadRequest);
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken)
        {
            var result = await _authService.VerifyEmailAsync(request.Email, request.Code, cancellationToken);

            return result.IsSuccess
                ? Ok(result.Value)
                : this.Problem(result.Error, StatusCodes.Status400BadRequest);
        }

        [HttpPost("resend-verification")]
        public async Task<IActionResult> ResendVerificationCodeAsync(ResendVerificationCodeRequest request, CancellationToken cancellationToken)
        {
            var result = await _authService.ResendVerificationCodeAsync(request.Email, cancellationToken);

            return result.IsSuccess
                ? Ok(result.Value)
                : this.Problem(result.Error, StatusCodes.Status400BadRequest);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            var result = await _authService.RefreshTokenAsync(request.Token, request.RefreshToken, cancellationToken);

            return result.IsSuccess
                ? Ok(result.Value)
                : this.Problem(result.Error, StatusCodes.Status400BadRequest);
        }

        [HttpPost("revoke")]
        public async Task<IActionResult> RevokeTokenAsync(RevokeTokenRequest request, CancellationToken cancellationToken)
        {
            var result = await _authService.RevokeRefreshTokenAsync(request.RefreshToken, cancellationToken);

            return result.IsSuccess
                ? Ok(new { message = "Token revoked successfully" })
                : this.Problem(result.Error, StatusCodes.Status400BadRequest);
        }
    }
}
