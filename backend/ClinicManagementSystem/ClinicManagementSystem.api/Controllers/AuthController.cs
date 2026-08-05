namespace ClinicManagementSystem.api.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService authService = authService;

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
        {
            var authResult = await authService.GetTokenAsync(request.Email, request.Password, cancellationToken);
            
            return authResult.IsSuccess 
                ? Ok(authResult.Value) 
                : BadRequest(new { error = authResult.Error.Message });
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
        {
            var authResult = await authService.RegisterAsync(request.FirstName, request.LastName, request.Email, request.Password, cancellationToken);
            
            return authResult.IsSuccess 
                ? Ok(authResult.Value) 
                : BadRequest(new { error = authResult.Error.Message });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            var authResult = await authService.RefreshTokenAsync(request.Token, request.RefreshToken, cancellationToken);
            
            return authResult.IsSuccess 
                ? Ok(authResult.Value) 
                : BadRequest(new { error = authResult.Error.Message });
        }

        [HttpPost("revoke")]
        public async Task<IActionResult> RevokeTokenAsync(RevokeTokenRequest request, CancellationToken cancellationToken)
        {
            var result = await authService.RevokeRefreshTokenAsync(request.RefreshToken, cancellationToken);
            
            return result.IsSuccess 
                ? Ok(new { message = "Token revoked successfully" }) 
                : BadRequest(new { error = result.Error.Message });
        }
    }
}
