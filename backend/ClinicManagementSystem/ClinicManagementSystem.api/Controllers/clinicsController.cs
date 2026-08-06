using ClinicManagementSystem.api.Contracts.Clinic;
using ClinicManagementSystem.api.Extensions;

namespace ClinicManagementSystem.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ClinicsController(IClinicService clinicService) : ControllerBase
    {
        private readonly IClinicService _clinicService = clinicService;

        [HttpGet("")]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var result = await _clinicService.GetAllAsync(cancellationToken);
            
            if (!result.IsSuccess)
                return this.Problem(result.Error, StatusCodes.Status400BadRequest);
            
            var response = result.Value.Adapt<List<ClinicResponse>>();
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        {
            var result = await _clinicService.GetAsync(id, cancellationToken);
            
            if (!result.IsSuccess)
                return this.Problem(result.Error, StatusCodes.Status404NotFound);

            var response = result.Value.Adapt<ClinicResponse>();
            return Ok(response);
        }

        [HttpPost("")]
        public async Task<IActionResult> Add(ClinicRequest request, CancellationToken cancellationToken)
        {
            var result = await _clinicService.AddAsync(request.Adapt<Clinic>(), cancellationToken);
            
            if (!result.IsSuccess)
            {
                var statusCode = result.Error.Type == ErrorType.Conflict 
                    ? StatusCodes.Status409Conflict 
                    : StatusCodes.Status400BadRequest;
                return this.Problem(result.Error, statusCode);
            }
            
            return CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ClinicRequest request, CancellationToken cancellationToken)
        {
            var result = await _clinicService.UpdateAsync(id, request.Adapt<Clinic>(), cancellationToken);
            
            if (!result.IsSuccess)
            {
                var statusCode = result.Error.Type == ErrorType.NotFound 
                    ? StatusCodes.Status404NotFound 
                    : StatusCodes.Status409Conflict;
                return this.Problem(result.Error, statusCode);
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var result = await _clinicService.DeleteAsync(id, cancellationToken);
            
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
}
