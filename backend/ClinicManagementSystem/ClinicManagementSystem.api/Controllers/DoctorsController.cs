using ClinicManagementSystem.api.Contracts.Doctor;
using ClinicManagementSystem.api.Extensions;

namespace ClinicManagementSystem.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DoctorsController(IDoctorService doctorService) : ControllerBase
    {
        private readonly IDoctorService _doctorService = doctorService;

        [HttpGet("")]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var result = await _doctorService.GetAllAsync(cancellationToken);
            
            if (!result.IsSuccess)
                return this.Problem(result.Error, StatusCodes.Status400BadRequest);
            
            var response = result.Value.Adapt<List<DoctorResponse>>();
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        {
            var result = await _doctorService.GetAsync(id, cancellationToken);
            
            if (!result.IsSuccess)
                return this.Problem(result.Error, StatusCodes.Status404NotFound);
            
            var response = result.Value.Adapt<DoctorResponse>();
            return Ok(response);
        }

        [HttpPost("")]
        public async Task<IActionResult> Add(DoctorRequest request, CancellationToken cancellationToken)
        {
            var result = await _doctorService.AddAsync(request.Adapt<Doctor>(), cancellationToken);
            
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
        public async Task<IActionResult> Update(int id, DoctorRequest request, CancellationToken cancellationToken)
        {
            var result = await _doctorService.UpdateAsync(id, request.Adapt<Doctor>(), cancellationToken);
            
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
