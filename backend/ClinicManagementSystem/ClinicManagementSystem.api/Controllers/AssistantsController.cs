using ClinicManagementSystem.api.Contracts.Assistant;
using ClinicManagementSystem.api.Extensions;

namespace ClinicManagementSystem.api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AssistantsController(IAssistantService assistantService) : ControllerBase
{
    private readonly IAssistantService _assistantService = assistantService;

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
            a.FirstName_En,
            a.FirstName_Ar,
            a.LastName_En,
            a.LastName_Ar,
            a.Phone,
            a.Email,
            a.IsActive,
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
            assistant.FirstName_En,
            assistant.FirstName_Ar,
            assistant.LastName_En,
            assistant.LastName_Ar,
            assistant.Phone,
            assistant.Email,
            assistant.IsActive,
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
            a.FirstName_En,
            a.FirstName_Ar,
            a.LastName_En,
            a.LastName_Ar,
            a.Phone,
            a.Email,
            a.IsActive,
            a.CreatedOn,
            a.UpdatedOn
        )).ToList();

        return Ok(response);
    }

    [HttpPost("")]
    public async Task<IActionResult> Add(AssistantRequest request, CancellationToken cancellationToken)
    {
        var assistant = new Models.Assistant
        {
            ClinicId = request.ClinicId,
            FirstName_En = request.FirstName_En,
            FirstName_Ar = request.FirstName_Ar,
            LastName_En = request.LastName_En,
            LastName_Ar = request.LastName_Ar,
            Phone = request.Phone,
            Email = request.Email,
            IsActive = request.IsActive
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

        return CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, AssistantRequest request, CancellationToken cancellationToken)
    {
        var assistant = new Models.Assistant
        {
            ClinicId = request.ClinicId,
            FirstName_En = request.FirstName_En,
            FirstName_Ar = request.FirstName_Ar,
            LastName_En = request.LastName_En,
            LastName_Ar = request.LastName_Ar,
            Phone = request.Phone,
            Email = request.Email,
            IsActive = request.IsActive
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
