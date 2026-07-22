using ClinicManagementSystem.api.Contracts.Clinic;
namespace ClinicManagementSystem.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ClinicsController(IClinicService clinicService) : ControllerBase
    {
        private readonly IClinicService _clinicService=clinicService;

        [HttpGet("")]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var clinics = await _clinicService.GetAllAsync(cancellationToken);
            var response = clinics.Adapt<List<ClinicResponse>>();
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        {
            var clinic =await _clinicService.GetAsync(id, cancellationToken);
            if (clinic == null)
                return NotFound();

            var response = clinic.Adapt<ClinicResponse>();
            return Ok(response);
        }

        [HttpPost("")]
        public async Task<IActionResult> Add(ClinicRequest request, CancellationToken cancellationToken)
        {
            var newClinic = await _clinicService.AddAsync(request.Adapt<Clinic>(),cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = newClinic.Id }, newClinic);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id,ClinicRequest request,CancellationToken cancellationToken)
        {
            var isUpdated =await _clinicService.UpdateAsync(id, request.Adapt<Clinic>(),cancellationToken);
            return isUpdated ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var isDeleted =await _clinicService.DeleteAsync(id);
            return isDeleted ? NoContent() : NotFound();
        }

    }
}
