namespace ClinicManagementSystem.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService authService = authService;

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
        {
            var authResult = await authService.GetTokenAsync(request.Email, request.Password, cancellationToken);
            return authResult is null ? BadRequest("Invalid email or password") : Ok(authResult);
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
        {
            var authResult = await authService.RegisterAsync(request.FirstName, request.LastName, request.Email, request.Password, cancellationToken);
            return authResult is null ? BadRequest("User already exists or registration failed") : Ok(authResult);
        }
    }
}
