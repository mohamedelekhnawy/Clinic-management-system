using ClinicManagementSystem.api.Contracts.Patient;
using ClinicManagementSystem.api.Contracts.Profile;
using ClinicManagementSystem.api.Extensions;

namespace ClinicManagementSystem.api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PatientsController(IPatientService patientService, IProfileService profileService) : ControllerBase
{
    private readonly IPatientService _patientService = patientService;
    private readonly IProfileService _profileService = profileService;

    [HttpGet("")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _patientService.GetAllAsync(cancellationToken);

        if (!result.IsSuccess)
            return this.Problem(result.Error, StatusCodes.Status400BadRequest);

        var response = result.Value.Select(p => new PatientResponse(
            p.Id,
            new ProfileResponse(
                p.Profile.Id,
                p.Profile.FirstName_En,
                p.Profile.FirstName_Ar,
                p.Profile.LastName_En,
                p.Profile.LastName_Ar,
                p.Profile.Phone,
                p.Profile.Email,
                p.Profile.IsActive,
                p.Profile.CreatedOn,
                p.Profile.UpdatedOn
            ),
            p.DateOfBirth,
            DateTime.Today.Year - p.DateOfBirth.Year,
            p.Gender.ToString(),
            p.Address_En,
            p.Address_Ar,
            p.EmergencyContactName,
            p.EmergencyContactPhone,
            p.Notes,
            p.CreatedOn,
            p.UpdatedOn
        )).ToList();

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
    {
        var result = await _patientService.GetAsync(id, cancellationToken);

        if (!result.IsSuccess)
            return this.Problem(result.Error, StatusCodes.Status404NotFound);

        var patient = result.Value;
        var response = new PatientResponse(
            patient.Id,
            new ProfileResponse(
                patient.Profile.Id,
                patient.Profile.FirstName_En,
                patient.Profile.FirstName_Ar,
                patient.Profile.LastName_En,
                patient.Profile.LastName_Ar,
                patient.Profile.Phone,
                patient.Profile.Email,
                patient.Profile.IsActive,
                patient.Profile.CreatedOn,
                patient.Profile.UpdatedOn
            ),
            patient.DateOfBirth,
            DateTime.Today.Year - patient.DateOfBirth.Year,
            patient.Gender.ToString(),
            patient.Address_En,
            patient.Address_Ar,
            patient.EmergencyContactName,
            patient.EmergencyContactPhone,
            patient.Notes,
            patient.CreatedOn,
            patient.UpdatedOn
        );

        return Ok(response);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string searchTerm, CancellationToken cancellationToken)
    {
        var result = await _patientService.SearchAsync(searchTerm, cancellationToken);

        if (!result.IsSuccess)
            return this.Problem(result.Error, StatusCodes.Status400BadRequest);

        var response = result.Value.Select(p => new PatientResponse(
            p.Id,
            new ProfileResponse(
                p.Profile.Id,
                p.Profile.FirstName_En,
                p.Profile.FirstName_Ar,
                p.Profile.LastName_En,
                p.Profile.LastName_Ar,
                p.Profile.Phone,
                p.Profile.Email,
                p.Profile.IsActive,
                p.Profile.CreatedOn,
                p.Profile.UpdatedOn
            ),
            p.DateOfBirth,
            DateTime.Today.Year - p.DateOfBirth.Year,
            p.Gender.ToString(),
            p.Address_En,
            p.Address_Ar,
            p.EmergencyContactName,
            p.EmergencyContactPhone,
            p.Notes,
            p.CreatedOn,
            p.UpdatedOn
        )).ToList();

        return Ok(response);
    }

    [HttpPost("")]
    public async Task<IActionResult> Add(PatientRequest request, CancellationToken cancellationToken)
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

        // Create patient with profile reference
        var patient = new Models.Patient
        {
            ProfileId = profileResult.Value.Id,
            DateOfBirth = request.DateOfBirth,
            Gender = (Gender)request.Gender,
            Address_En = request.Address_En,
            Address_Ar = request.Address_Ar,
            EmergencyContactName = request.EmergencyContactName,
            EmergencyContactPhone = request.EmergencyContactPhone,
            Notes = request.Notes
        };

        var result = await _patientService.AddAsync(patient, cancellationToken);

        if (!result.IsSuccess)
        {
            var statusCode = result.Error.Type switch
            {
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status400BadRequest
            };
            return this.Problem(result.Error, statusCode);
        }

        return CreatedAtAction(nameof(Get), new { id = result.Value.Id }, new PatientResponse(
            result.Value.Id,
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
            result.Value.DateOfBirth,
            DateTime.Today.Year - result.Value.DateOfBirth.Year,
            result.Value.Gender.ToString(),
            result.Value.Address_En,
            result.Value.Address_Ar,
            result.Value.EmergencyContactName,
            result.Value.EmergencyContactPhone,
            result.Value.Notes,
            result.Value.CreatedOn,
            result.Value.UpdatedOn
        ));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, PatientRequest request, CancellationToken cancellationToken)
    {
        // Get existing patient to get ProfileId
        var existingResult = await _patientService.GetAsync(id, cancellationToken);
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

        // Update patient
        var patient = new Models.Patient
        {
            DateOfBirth = request.DateOfBirth,
            Gender = (Gender)request.Gender,
            Address_En = request.Address_En,
            Address_Ar = request.Address_Ar,
            EmergencyContactName = request.EmergencyContactName,
            EmergencyContactPhone = request.EmergencyContactPhone,
            Notes = request.Notes
        };

        var result = await _patientService.UpdateAsync(id, patient, cancellationToken);

        if (!result.IsSuccess)
        {
            var statusCode = result.Error.Type switch
            {
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status400BadRequest
            };
            return this.Problem(result.Error, statusCode);
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _patientService.DeleteAsync(id, cancellationToken);

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
