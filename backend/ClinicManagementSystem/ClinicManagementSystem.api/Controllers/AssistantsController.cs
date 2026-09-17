using ClinicManagementSystem.api.Contracts.Assistant;
using ClinicManagementSystem.api.Contracts.Profile;
using ClinicManagementSystem.api.Extensions;

namespace ClinicManagementSystem.api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AssistantsController(IAssistantService assistantService, IProfileService profileService) : ControllerBase
{
    private readonly IAssistantService _assistantService = assistantService;
    private readonly IProfileService _profileService = profileService;

    [HttpGet("")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _assistantService.GetAllAsync(cancellationToken);

        if (!result.IsSuccess)
            return this.Problem(result.Error, StatusCodes.Status400BadRequest);

        var response = result.Value.Select(a => new AssistantResponse(
            a.Id,
            a.ClinicId,
            a.Clinic.Name_En,
            a.Clinic.Name_Ar,
            new ProfileResponse(
                a.Profile.Id,
                a.Profile.FirstName_En,
                a.Profile.FirstName_Ar,
                a.Profile.LastName_En,
                a.Profile.LastName_Ar,
                a.Profile.Phone,
                a.Profile.Email,
                a.Profile.IsActive,
                a.Profile.CreatedOn,
                a.Profile.UpdatedOn
            ),
            a.CreatedOn,
            a.UpdatedOn
        )).ToList();

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
    {
        var result = await _assistantService.GetAsync(id, cancellationToken);

        if (!result.IsSuccess)
            return this.Problem(result.Error, StatusCodes.Status404NotFound);

        var assistant = result.Value;
        var response = new AssistantResponse(
            assistant.Id,
            assistant.ClinicId,
            assistant.Clinic.Name_En,
            assistant.Clinic.Name_Ar,
            new ProfileResponse(
                assistant.Profile.Id,
                assistant.Profile.FirstName_En,
                assistant.Profile.FirstName_Ar,
                assistant.Profile.LastName_En,
                assistant.Profile.LastName_Ar,
                assistant.Profile.Phone,
                assistant.Profile.Email,
                assistant.Profile.IsActive,
                assistant.Profile.CreatedOn,
                assistant.Profile.UpdatedOn
            ),
            assistant.CreatedOn,
            assistant.UpdatedOn
        );

        return Ok(response);
    }

    [HttpGet("clinic/{clinicId}")]
    public async Task<IActionResult> GetByClinic(int clinicId, CancellationToken cancellationToken)
    {
        var result = await _assistantService.GetByClinicIdAsync(clinicId, cancellationToken);

        if (!result.IsSuccess)
            return this.Problem(result.Error, StatusCodes.Status400BadRequest);

        var response = result.Value.Select(a => new AssistantResponse(
            a.Id,
            a.ClinicId,
            a.Clinic.Name_En,
            a.Clinic.Name_Ar,
            new ProfileResponse(
                a.Profile.Id,
                a.Profile.FirstName_En,
                a.Profile.FirstName_Ar,
                a.Profile.LastName_En,
                a.Profile.LastName_Ar,
                a.Profile.Phone,
                a.Profile.Email,
                a.Profile.IsActive,
                a.Profile.CreatedOn,
                a.Profile.UpdatedOn
            ),
            a.CreatedOn,
            a.UpdatedOn
        )).ToList();

        return Ok(response);
    }

    [HttpPost("")]
    public async Task<IActionResult> Add(AssistantRequest request, CancellationToken cancellationToken)
    {
        // Create profile first
        var profile = new Models.Profile
        {
            FirstName_En = request.Profile.FirstName_En,
            FirstName_Ar = request.Profile.FirstName_Ar,
            LastName_En = request.Profile.LastName_En,
            LastName_Ar = request.Profile.LastName_Ar,
            Phone = request.Profile.Phone,
            Email = request.Profile.Email,
            IsActive = request.Profile.IsActive
        };

        var profileResult = await _profileService.AddAsync(profile, cancellationToken);
        if (!profileResult.IsSuccess)
        {
            var statusCode = profileResult.Error.Type switch
            {
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status400BadRequest
            };
            return this.Problem(profileResult.Error, statusCode);
        }

        // Create assistant with profile reference
        var assistant = new Models.Assistant
        {
            ClinicId = request.ClinicId,
            ProfileId = profileResult.Value.Id
        };

        var result = await _assistantService.AddAsync(assistant, cancellationToken);

        if (!result.IsSuccess)
        {
            var statusCode = result.Error.Type switch
            {
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status400BadRequest
            };
            return this.Problem(result.Error, statusCode);
        }

        return CreatedAtAction(nameof(Get), new { id = result.Value.Id }, new AssistantResponse(
            result.Value.Id,
            result.Value.ClinicId,
            result.Value.Clinic.Name_En,
            result.Value.Clinic.Name_Ar,
            new ProfileResponse(
                result.Value.Profile.Id,
                result.Value.Profile.FirstName_En,
                result.Value.Profile.FirstName_Ar,
                result.Value.Profile.LastName_En,
                result.Value.Profile.LastName_Ar,
                result.Value.Profile.Phone,
                result.Value.Profile.Email,
                result.Value.Profile.IsActive,
                result.Value.Profile.CreatedOn,
                result.Value.Profile.UpdatedOn
            ),
            result.Value.CreatedOn,
            result.Value.UpdatedOn
        ));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, AssistantRequest request, CancellationToken cancellationToken)
    {
        // Get existing assistant to get ProfileId
        var existingResult = await _assistantService.GetAsync(id, cancellationToken);
        if (!existingResult.IsSuccess)
            return this.Problem(existingResult.Error, StatusCodes.Status404NotFound);

        // Update profile
        var profile = new Models.Profile
        {
            FirstName_En = request.Profile.FirstName_En,
            FirstName_Ar = request.Profile.FirstName_Ar,
            LastName_En = request.Profile.LastName_En,
            LastName_Ar = request.Profile.LastName_Ar,
            Phone = request.Profile.Phone,
            Email = request.Profile.Email,
            IsActive = request.Profile.IsActive
        };

        var profileResult = await _profileService.UpdateAsync(existingResult.Value.ProfileId, profile, cancellationToken);
        if (!profileResult.IsSuccess)
        {
            var statusCode = profileResult.Error.Type switch
            {
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status400BadRequest
            };
            return this.Problem(profileResult.Error, statusCode);
        }

        // Update assistant
        var assistant = new Models.Assistant
        {
            ClinicId = request.ClinicId
        };

        var result = await _assistantService.UpdateAsync(id, assistant, cancellationToken);

        if (!result.IsSuccess)
        {
            var statusCode = result.Error.Type switch
            {
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status400BadRequest
            };
            return this.Problem(result.Error, statusCode);
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _assistantService.DeleteAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            var statusCode = result.Error.Type == ErrorType.NotFound
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status409Conflict;
            return this.Problem(result.Error, statusCode);
        }

        return NoContent();
    }
}
