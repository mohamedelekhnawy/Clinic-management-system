using ClinicManagementSystem.api.Abstractions;
using ClinicManagementSystem.api.Models;
using ClinicManagementSystem.api.Models.Enums;

namespace ClinicManagementSystem.api.Services;

public interface IAppointmentService
{
    // ========== Basic CRUD ==========
    Task<Result<IEnumerable<Appointment>>> GetAllAsync(
        int? doctorId,
        int? patientId,
        DateOnly? date,
        AppointmentStatus? status,
        CancellationToken cancellationToken);

    Task<Result<Appointment>> GetAsync(int id, CancellationToken cancellationToken);

    Task<Result<Appointment>> CreateAsync(Appointment appointment, CancellationToken cancellationToken);

    Task<Result> UpdateAsync(int id, Appointment appointment, CancellationToken cancellationToken);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken);

    // ========== Lifecycle Actions ==========
    Task<Result> CheckInAsync(int id, CancellationToken cancellationToken);

    Task<Result> StartAppointmentAsync(int id, CancellationToken cancellationToken);

    Task<Result> CompleteAppointmentAsync(int id, CancellationToken cancellationToken);

    Task<Result> CancelAppointmentAsync(int id, CancellationToken cancellationToken);

    // ========== Queue Management ==========
    Task<Result<QueueInfo>> GetDoctorQueueAsync(int doctorId, DateOnly date, CancellationToken cancellationToken);
}

public record QueueInfo(
    Appointment? CurrentPatient,
    List<Appointment> WaitingPatients,
    int TotalWaiting,
    int EstimatedWaitingMinutes,
    Doctor Doctor
);
