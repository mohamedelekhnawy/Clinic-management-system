using ClinicManagementSystem.api.Contracts.Doctor;

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
                return BadRequest(new { error = result.Error.Message });
            
            var response = result.Value.Adapt<List<DoctorResponse>>();
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        {
            var result = await _doctorService.GetAsync(id, cancellationToken);
            
            if (!result.IsSuccess)
                return NotFound(new { error = result.Error.Message });
            
            var response = result.Value.Adapt<DoctorResponse>();
            return Ok(response);
        }

        [HttpPost("")]
        public async Task<IActionResult> Add(DoctorRequest request, CancellationToken cancellationToken)
        {
            var result = await _doctorService.AddAsync(request.Adapt<Doctor>(), cancellationToken);
            
            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error.Message });
            
            return CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, DoctorRequest request, CancellationToken cancellationToken)
        {
            var result = await _doctorService.UpdateAsync(id, request.Adapt<Doctor>(), cancellationToken);
            
            return result.IsSuccess 
                ? NoContent() 
                : NotFound(new { error = result.Error.Message });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var result = await _doctorService.DeleteAsync(id, cancellationToken);
            
            return result.IsSuccess 
                ? NoContent() 
                : NotFound(new { error = result.Error.Message });
        }
    }
}
