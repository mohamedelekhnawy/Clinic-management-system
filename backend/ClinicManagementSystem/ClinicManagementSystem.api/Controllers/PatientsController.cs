using ClinicManagementSystem.api.Contracts.Patient;
using ClinicManagementSystem.api.Extensions;

namespace ClinicManagementSystem.api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PatientsController(IPatientService patientService) : ControllerBase
{
    private readonly IPatientService _patientService = patientService;

    [HttpGet("")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _patientService.GetAllAsync(cancellationToken);

        if (!result.IsSuccess)
            return this.Problem(result.Error, StatusCodes.Status400BadRequest);

        var response = result.Value.Select(p => new PatientResponse(
            p.Id,
            p.FirstName_En,
            p.FirstName_Ar,
            p.LastName_En,
            p.LastName_Ar,
            p.DateOfBirth,
            DateTime.Today.Year - p.DateOfBirth.Year,
            p.Gender.ToString(),
            p.Phone,
            p.Email,
            p.Address_En,
            p.Address_Ar,
            p.EmergencyContactName,
            p.EmergencyContactPhone,
            p.Notes,
            p.IsActive,
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
            patient.FirstName_En,
            patient.FirstName_Ar,
            patient.LastName_En,
            patient.LastName_Ar,
            patient.DateOfBirth,
            DateTime.Today.Year - patient.DateOfBirth.Year,
            patient.Gender.ToString(),
            patient.Phone,
            patient.Email,
            patient.Address_En,
            patient.Address_Ar,
            patient.EmergencyContactName,
            patient.EmergencyContactPhone,
            patient.Notes,
            patient.IsActive,
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
            p.FirstName_En,
            p.FirstName_Ar,
            p.LastName_En,
            p.LastName_Ar,
            p.DateOfBirth,
            DateTime.Today.Year - p.DateOfBirth.Year,
            p.Gender.ToString(),
            p.Phone,
            p.Email,
            p.Address_En,
            p.Address_Ar,
            p.EmergencyContactName,
            p.EmergencyContactPhone,
            p.Notes,
            p.IsActive,
            p.CreatedOn,
            p.UpdatedOn
        )).ToList();

        return Ok(response);
    }

    [HttpPost("")]
    public async Task<IActionResult> Add(PatientRequest request, CancellationToken cancellationToken)
    {
        var patient = new Models.Patient
        {
            FirstName_En = request.FirstName_En,
            FirstName_Ar = request.FirstName_Ar,
            LastName_En = request.LastName_En,
            LastName_Ar = request.LastName_Ar,
            DateOfBirth = request.DateOfBirth,
            Gender = (Gender)request.Gender,
            Phone = request.Phone,
            Email = request.Email,
            Address_En = request.Address_En,
            Address_Ar = request.Address_Ar,
            EmergencyContactName = request.EmergencyContactName,
            EmergencyContactPhone = request.EmergencyContactPhone,
            Notes = request.Notes,
            IsActive = request.IsActive
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

        return CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, PatientRequest request, CancellationToken cancellationToken)
    {
        var patient = new Models.Patient
        {
            FirstName_En = request.FirstName_En,
            FirstName_Ar = request.FirstName_Ar,
            LastName_En = request.LastName_En,
            LastName_Ar = request.LastName_Ar,
            DateOfBirth = request.DateOfBirth,
            Gender = (Gender)request.Gender,
            Phone = request.Phone,
            Email = request.Email,
            Address_En = request.Address_En,
            Address_Ar = request.Address_Ar,
            EmergencyContactName = request.EmergencyContactName,
            EmergencyContactPhone = request.EmergencyContactPhone,
            Notes = request.Notes,
            IsActive = request.IsActive
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
