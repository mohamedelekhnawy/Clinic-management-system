using ClinicManagementSystem.api.Contracts.Request;
using ClinicManagementSystem.api.Contracts.Responce;
using ClinicManagementSystem.api.Models;
using ClinicManagementSystem.api.Services;
using Mapster;
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
            var clinics = _clinicService.GetAll();
            var response = clinics.Adapt<List<ClinicResponse>>();
            return Ok(response);
        }
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var clinic = _clinicService.Get(id);
            if (clinic == null)
                return NotFound();

            var response = clinic.Adapt<ClinicResponse>();
            return Ok(response);
        }

        [HttpPost("")]
        public IActionResult Add(CreateClinicRequest request)
        {
            var newClinic = _clinicService.Add(request.Adapt<Clinic>());
            return CreatedAtAction(nameof(Get), new{ id= newClinic.Id },newClinic);
        }
        [HttpPut("{id}")]
        public IActionResult Update(int id, CreateClinicRequest request)
        {
            var isUpdated = _clinicService.Update(id, request.Adapt<Clinic>());
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
