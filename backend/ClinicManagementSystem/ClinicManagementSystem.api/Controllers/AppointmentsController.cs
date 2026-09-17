using ClinicManagementSystem.api.Abstractions;
using ClinicManagementSystem.api.Contracts.Appointment;
using ClinicManagementSystem.api.Extensions;
using ClinicManagementSystem.api.Models;
using ClinicManagementSystem.api.Models.Enums;
using ClinicManagementSystem.api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagementSystem.api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AppointmentsController(IAppointmentService appointmentService) : ControllerBase
{
    private readonly IAppointmentService _appointmentService = appointmentService;

    // ========== Basic CRUD ==========

    [HttpGet("")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? doctorId,
        [FromQuery] int? patientId,
        [FromQuery] DateOnly? date,
        [FromQuery] AppointmentStatus? status,
        CancellationToken cancellationToken)
    {
        var result = await _appointmentService.GetAllAsync(doctorId, patientId, date, status, cancellationToken);

        if (!result.IsSuccess)
            return this.Problem(result.Error, StatusCodes.Status400BadRequest);

        return Ok(result.Value.Select(MapToResponse).ToList());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
    {
        var result = await _appointmentService.GetAsync(id, cancellationToken);

        if (!result.IsSuccess)
            return this.Problem(result.Error, StatusCodes.Status404NotFound);

        return Ok(MapToResponse(result.Value));
    }

    [HttpPost("")]
    public async Task<IActionResult> Create(AppointmentRequest request, CancellationToken cancellationToken)
    {
        var appointment = MapToEntity(request);
        var result = await _appointmentService.CreateAsync(appointment, cancellationToken);

        if (!result.IsSuccess)
            return this.Problem(result.Error, ToStatusCode(result.Error.Type));

        return CreatedAtAction(nameof(Get), new { id = result.Value.Id }, MapToResponse(result.Value));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, AppointmentRequest request, CancellationToken cancellationToken)
    {
        var appointment = MapToEntity(request);
        var result = await _appointmentService.UpdateAsync(id, appointment, cancellationToken);

        if (!result.IsSuccess)
            return this.Problem(result.Error, ToStatusCode(result.Error.Type));

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _appointmentService.DeleteAsync(id, cancellationToken);

        if (!result.IsSuccess)
            return this.Problem(result.Error, ToStatusCode(result.Error.Type));

        return NoContent();
    }

    // ========== Lifecycle Actions ==========

    [HttpPost("{id}/check-in")]
    public async Task<IActionResult> CheckIn(int id, CancellationToken cancellationToken)
    {
        var result = await _appointmentService.CheckInAsync(id, cancellationToken);

        if (!result.IsSuccess)
            return this.Problem(result.Error, ToStatusCode(result.Error.Type));

        return NoContent();
    }

    [HttpPost("{id}/start")]
    public async Task<IActionResult> Start(int id, CancellationToken cancellationToken)
    {
        var result = await _appointmentService.StartAppointmentAsync(id, cancellationToken);

        if (!result.IsSuccess)
            return this.Problem(result.Error, ToStatusCode(result.Error.Type));

        return NoContent();
    }

    [HttpPost("{id}/complete")]
    public async Task<IActionResult> Complete(int id, CancellationToken cancellationToken)
    {
        var result = await _appointmentService.CompleteAppointmentAsync(id, cancellationToken);

        if (!result.IsSuccess)
            return this.Problem(result.Error, ToStatusCode(result.Error.Type));

        return NoContent();
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        var result = await _appointmentService.CancelAppointmentAsync(id, cancellationToken);

        if (!result.IsSuccess)
            return this.Problem(result.Error, ToStatusCode(result.Error.Type));

        return NoContent();
    }

    // ========== Queue Management ==========

    [HttpGet("doctor/{doctorId}/queue")]
    public async Task<IActionResult> GetDoctorQueue(
        int doctorId,
        [FromQuery] DateOnly? date,
        CancellationToken cancellationToken)
    {
        var targetDate = date ?? DateOnly.FromDateTime(DateTime.Today);
        var result = await _appointmentService.GetDoctorQueueAsync(doctorId, targetDate, cancellationToken);

        if (!result.IsSuccess)
            return this.Problem(result.Error, ToStatusCode(result.Error.Type));

        var queue = result.Value;
        var doctor = queue.Doctor;

        QueueItemResponse? currentResponse = queue.CurrentPatient is null
            ? null
            : MapToQueueItem(queue.CurrentPatient, positionInQueue: 0);

        var waitingResponse = queue.WaitingPatients
            .Select((a, index) => MapToQueueItem(a, index + 1))
            .ToList();

        var response = new QueueResponse(
            doctorId,
            $"{doctor.Profile.FirstName_En} {doctor.Profile.LastName_En}",
            $"{doctor.Profile.FirstName_Ar} {doctor.Profile.LastName_Ar}",
            currentResponse,
            waitingResponse,
            queue.TotalWaiting,
            queue.EstimatedWaitingMinutes
        );

        return Ok(response);
    }

    private static Appointment MapToEntity(AppointmentRequest request) => new()
    {
        PatientId = request.PatientId,
        DoctorId = request.DoctorId,
        AppointmentDate = request.AppointmentDate,
        StartTime = request.StartTime,
        EndTime = request.EndTime,
        Notes = request.Notes
    };

    private static AppointmentResponse MapToResponse(Appointment a) => new(
        a.Id,
        a.PatientId,
        $"{a.Patient.Profile.FirstName_En} {a.Patient.Profile.LastName_En}",
        $"{a.Patient.Profile.FirstName_Ar} {a.Patient.Profile.LastName_Ar}",
        a.DoctorId,
        $"{a.Doctor.Profile.FirstName_En} {a.Doctor.Profile.LastName_En}",
        $"{a.Doctor.Profile.FirstName_Ar} {a.Doctor.Profile.LastName_Ar}",
        a.AppointmentDate,
        a.StartTime,
        a.EndTime,
        a.Status.ToString(),
        a.CheckedInAt,
        a.ActualStartTime,
        a.ActualEndTime,
        a.Notes,
        a.CreatedOn,
        a.UpdatedOn
    );

    private static QueueItemResponse MapToQueueItem(Appointment a, int positionInQueue) => new(
        a.Id,
        a.PatientId,
        $"{a.Patient.Profile.FirstName_En} {a.Patient.Profile.LastName_En}",
        $"{a.Patient.Profile.FirstName_Ar} {a.Patient.Profile.LastName_Ar}",
        a.StartTime,
        a.CheckedInAt ?? DateTime.MinValue,
        positionInQueue
    );

    private static int ToStatusCode(ErrorType type) => type switch
    {
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        _ => StatusCodes.Status400BadRequest
    };
}
