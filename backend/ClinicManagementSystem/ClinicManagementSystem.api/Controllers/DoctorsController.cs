using ClinicManagementSystem.api.Contracts.Doctor;
using ClinicManagementSystem.api.Contracts.Profile;
using ClinicManagementSystem.api.Extensions;

namespace ClinicManagementSystem.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DoctorsController(IDoctorService doctorService, IProfileService profileService) : ControllerBase
    {
        private readonly IDoctorService _doctorService = doctorService;
        private readonly IProfileService _profileService = profileService;

        [HttpGet("")]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var result = await _doctorService.GetAllAsync(cancellationToken);
            
            if (!result.IsSuccess)
                return this.Problem(result.Error, StatusCodes.Status400BadRequest);
            
            var response = result.Value.Select(d => new DoctorResponse(
                d.Id,
                d.ClinicId,
                new ProfileResponse(
                    d.Profile.Id,
                    d.Profile.FirstName_En,
                    d.Profile.FirstName_Ar,
                    d.Profile.LastName_En,
                    d.Profile.LastName_Ar,
                    d.Profile.Phone,
                    d.Profile.Email,
                    d.Profile.IsActive,
                    d.Profile.CreatedOn,
                    d.Profile.UpdatedOn
                ),
                d.Specialty_En,
                d.Specialty_Ar,
                d.Description_En,
                d.Description_Ar,
                d.SessionPrice,
                d.CreatedOn,
                d.CreatedBy,
                d.UpdatedOn,
                d.UpdatedBy
            )).ToList();
            
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        {
            var result = await _doctorService.GetAsync(id, cancellationToken);
            
            if (!result.IsSuccess)
                return this.Problem(result.Error, StatusCodes.Status404NotFound);
            
            var d = result.Value;
            var response = new DoctorResponse(
                d.Id,
                d.ClinicId,
                new ProfileResponse(
                    d.Profile.Id,
                    d.Profile.FirstName_En,
                    d.Profile.FirstName_Ar,
                    d.Profile.LastName_En,
                    d.Profile.LastName_Ar,
                    d.Profile.Phone,
                    d.Profile.Email,
                    d.Profile.IsActive,
                    d.Profile.CreatedOn,
                    d.Profile.UpdatedOn
                ),
                d.Specialty_En,
                d.Specialty_Ar,
                d.Description_En,
                d.Description_Ar,
                d.SessionPrice,
                d.CreatedOn,
                d.CreatedBy,
                d.UpdatedOn,
                d.UpdatedBy
            );
            
            return Ok(response);
        }

        [HttpPost("")]
        public async Task<IActionResult> Add(DoctorRequest request, CancellationToken cancellationToken)
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

            // Create doctor with profile reference
            var doctor = new Doctor
            {
                ClinicId = request.ClinicId,
                ProfileId = profileResult.Value.Id,
                Specialty_En = request.Specialty_En,
                Specialty_Ar = request.Specialty_Ar,
                Description_En = request.Description_En,
                Description_Ar = request.Description_Ar,
                SessionPrice = request.SessionPrice
            };
            
            var result = await _doctorService.AddAsync(doctor, cancellationToken);
            
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
            
            return CreatedAtAction(nameof(Get), new { id = result.Value.Id }, new DoctorResponse(
                result.Value.Id,
                result.Value.ClinicId,
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
                result.Value.Specialty_En,
                result.Value.Specialty_Ar,
                result.Value.Description_En,
                result.Value.Description_Ar,
                result.Value.SessionPrice,
                result.Value.CreatedOn,
                result.Value.CreatedBy,
                result.Value.UpdatedOn,
                result.Value.UpdatedBy
            ));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, DoctorRequest request, CancellationToken cancellationToken)
        {
            // Get existing doctor to get ProfileId
            var existingResult = await _doctorService.GetAsync(id, cancellationToken);
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

            // Update doctor
            var doctor = new Doctor
            {
                ClinicId = request.ClinicId,
                Specialty_En = request.Specialty_En,
                Specialty_Ar = request.Specialty_Ar,
                Description_En = request.Description_En,
                Description_Ar = request.Description_Ar,
                SessionPrice = request.SessionPrice
            };
            
            var result = await _doctorService.UpdateAsync(id, doctor, cancellationToken);
            
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
            var result = await _doctorService.DeleteAsync(id, cancellationToken);
            
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
    }
}
