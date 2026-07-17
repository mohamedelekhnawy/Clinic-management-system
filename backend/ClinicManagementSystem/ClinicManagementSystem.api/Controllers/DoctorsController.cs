namespace ClinicManagementSystem.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorsController(IDoctorService doctorService) : ControllerBase
    {
        private readonly IDoctorService _doctorService = doctorService;

        [HttpGet("")]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var doctors = await _doctorService.GetAllAsync(cancellationToken);
            var response = doctors.Adapt<List<DoctorResponse>>();
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id,CancellationToken cancellationToken) 
        { 
            var doctor =await _doctorService.GetAsync(id,cancellationToken);
            if (doctor == null)
                return NotFound();
            var response = doctor.Adapt<DoctorResponse>();
            return Ok(response);
        }
        [HttpPost("")]
        public async Task<IActionResult> Add(DoctorRequest request,CancellationToken cancellationToken)
        {
            var newDoctor = await _doctorService.AddAsync(request.Adapt<Doctor>(),cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = newDoctor.Id }, newDoctor);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, DoctorRequest request, CancellationToken cancellationToken)
        {
            var isUpdated = await _doctorService.UpdateAsync(id, request.Adapt<Doctor>(), cancellationToken);
            return isUpdated ? NoContent() : NotFound();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id ,CancellationToken cancellationToken)
        {
            var isDeleted = await _doctorService.DeleteAsync(id, cancellationToken);
            return isDeleted ? NoContent() : NotFound();
        }
    }
}
