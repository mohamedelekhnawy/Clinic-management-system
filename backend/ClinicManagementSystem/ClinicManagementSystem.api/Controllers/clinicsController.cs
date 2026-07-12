using ClinicManagementSystem.api.Models;
using ClinicManagementSystem.api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagementSystem.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class clinicsController : ControllerBase
    {
        private readonly IClinicService _clinicService;

        public clinicsController(IClinicService clinicService)
        {
            _clinicService = clinicService;
        }

        [HttpGet ("")]
        public IActionResult GetAll()
        {
            return Ok(_clinicService.GetAll());
        }
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var clinic = _clinicService.Get(id);
            return clinic is null ? NotFound() : Ok(clinic);
        }
        [HttpPost("")]
        public IActionResult Add(Clinic request)
        {
            var newClinic = _clinicService.Add(request);
            return CreatedAtAction(nameof(Get), new{ id= newClinic.Id },newClinic);
        }
        [HttpPut("{id}")]
        public IActionResult Update(int id, Clinic request)
        {
            var isUpdated = _clinicService.Update(id, request);
            return isUpdated ? NoContent() : NotFound();
        }
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var isDeleted = _clinicService.Delete(id);
            return isDeleted ? NoContent() : NotFound();
        }

    }
}
