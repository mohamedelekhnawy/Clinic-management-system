using ClinicManagementSystem.api.Contracts.Clinic;
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
                return BadRequest(new { error = result.Error.Message });
            
            var response = result.Value.Adapt<List<ClinicResponse>>();
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        {
            var result = await _clinicService.GetAsync(id, cancellationToken);
            
            if (!result.IsSuccess)
                return NotFound(new { error = result.Error.Message });

            var response = result.Value.Adapt<ClinicResponse>();
            return Ok(response);
        }

        [HttpPost("")]
        public async Task<IActionResult> Add(ClinicRequest request, CancellationToken cancellationToken)
        {
            var result = await _clinicService.AddAsync(request.Adapt<Clinic>(), cancellationToken);
            
            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error.Message });
            
            return CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ClinicRequest request, CancellationToken cancellationToken)
        {
            var result = await _clinicService.UpdateAsync(id, request.Adapt<Clinic>(), cancellationToken);
            
            return result.IsSuccess 
                ? NoContent() 
                : NotFound(new { error = result.Error.Message });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var result = await _clinicService.DeleteAsync(id, cancellationToken);
            
            return result.IsSuccess 
                ? NoContent() 
                : NotFound(new { error = result.Error.Message });
        }

    }
}
