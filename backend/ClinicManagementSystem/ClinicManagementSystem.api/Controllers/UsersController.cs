using ClinicManagementSystem.api.Contracts.Profile;
using ClinicManagementSystem.api.Extensions;

namespace ClinicManagementSystem.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController(IUserService userService, ICurrentUserService currentUserService) : ControllerBase
    {
        private readonly IUserService _userService = userService;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        [HttpGet("me")]
        public async Task<IActionResult> GetProfileAsync(CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
                return this.Problem(AuthErrors.InvalidToken, StatusCodes.Status401Unauthorized);

            var result = await _userService.GetProfileAsync(userId, cancellationToken);

            return result.IsSuccess
                ? Ok(result.Value)
                : this.Problem(result.Error, StatusCodes.Status404NotFound);
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateProfileAsync(UpdateProfileRequest request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
                return this.Problem(AuthErrors.InvalidToken, StatusCodes.Status401Unauthorized);

            var result = await _userService.UpdateProfileAsync(userId, request, cancellationToken);

            return result.IsSuccess
                ? Ok(result.Value)
                : result.Error.Type == ErrorType.NotFound
                    ? this.Problem(result.Error, StatusCodes.Status404NotFound)
                    : this.Problem(result.Error, StatusCodes.Status409Conflict);
        }
    }
}
