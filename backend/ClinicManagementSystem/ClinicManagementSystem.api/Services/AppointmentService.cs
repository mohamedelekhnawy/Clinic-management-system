using ClinicManagementSystem.api.Abstractions;
using ClinicManagementSystem.api.Models;
using ClinicManagementSystem.api.Models.Enums;
using ClinicManagementSystem.api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagementSystem.api.Services;

public class AppointmentService(ApplicationDbContext context, IConfiguration configuration) : IAppointmentService
{
    private readonly ApplicationDbContext _context = context;
    private readonly int _consultationDurationMinutes = configuration.GetValue(
        "Appointment:DefaultConsultationDurationMinutes", 30);

    // ========== Basic CRUD ==========

    public async Task<Result<IEnumerable<Appointment>>> GetAllAsync(
        int? doctorId,
        int? patientId,
        DateOnly? date,
        AppointmentStatus? status,
        CancellationToken cancellationToken)
    {
        var query = _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .AsNoTracking();

        if (doctorId.HasValue)
            query = query.Where(a => a.DoctorId == doctorId.Value);

        if (patientId.HasValue)
            query = query.Where(a => a.PatientId == patientId.Value);

        if (date.HasValue)
            query = query.Where(a => a.AppointmentDate == date.Value);

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        var appointments = await query
            .OrderByDescending(a => a.AppointmentDate)
            .ThenBy(a => a.StartTime)
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<Appointment>>(appointments);
    }

    public async Task<Result<Appointment>> GetAsync(int id, CancellationToken cancellationToken)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        return appointment is null
            ? Result.Failure<Appointment>(AppointmentErrors.NotFound)
            : Result.Success(appointment);
    }

    public async Task<Result<Appointment>> CreateAsync(Appointment appointment, CancellationToken cancellationToken)
    {
        var validationError = await ValidateBookingAsync(appointment, excludeAppointmentId: null, cancellationToken);
        if (validationError is not null)
            return Result.Failure<Appointment>(validationError);

        appointment.Status = AppointmentStatus.Scheduled;

        await _context.Appointments.AddAsync(appointment, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var created = await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .FirstAsync(a => a.Id == appointment.Id, cancellationToken);

        return Result.Success(created);
    }

    public async Task<Result> UpdateAsync(int id, Appointment appointment, CancellationToken cancellationToken)
    {
        var existing = await _context.Appointments.FindAsync([id], cancellationToken);
        if (existing is null)
            return Result.Failure(AppointmentErrors.NotFound);

        if (existing.Status != AppointmentStatus.Scheduled && existing.Status != AppointmentStatus.Confirmed)
            return Result.Failure(AppointmentErrors.CannotUpdate);

        var validationError = await ValidateBookingAsync(appointment, excludeAppointmentId: id, cancellationToken);
        if (validationError is not null)
            return Result.Failure(validationError);

        existing.PatientId = appointment.PatientId;
        existing.DoctorId = appointment.DoctorId;
        existing.AppointmentDate = appointment.AppointmentDate;
        existing.StartTime = appointment.StartTime;
        existing.EndTime = appointment.EndTime;
        existing.Notes = appointment.Notes;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            var exists = await _context.Appointments.AnyAsync(a => a.Id == id, cancellationToken);
            if (!exists)
                return Result.Failure(AppointmentErrors.NotFound);
            throw;
        }
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var appointment = await _context.Appointments.FindAsync([id], cancellationToken);
        if (appointment is null)
            return Result.Failure(AppointmentErrors.NotFound);

        if (appointment.Status != AppointmentStatus.Scheduled && appointment.Status != AppointmentStatus.Cancelled)
            return Result.Failure(AppointmentErrors.CannotDelete);

        _context.Remove(appointment);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    // ========== Lifecycle Actions ==========

    public async Task<Result> CheckInAsync(int id, CancellationToken cancellationToken)
    {
        var appointment = await _context.Appointments.FindAsync([id], cancellationToken);
        if (appointment is null)
            return Result.Failure(AppointmentErrors.NotFound);

        if (appointment.Status != AppointmentStatus.Scheduled && appointment.Status != AppointmentStatus.Confirmed)
            return Result.Failure(AppointmentErrors.CannotCheckIn);

        if (appointment.CheckedInAt.HasValue)
            return Result.Failure(AppointmentErrors.AlreadyCheckedIn);

        appointment.CheckedInAt = DateTime.UtcNow;
        appointment.Status = AppointmentStatus.Waiting;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> StartAppointmentAsync(int id, CancellationToken cancellationToken)
    {
        var appointment = await _context.Appointments.FindAsync([id], cancellationToken);
        if (appointment is null)
            return Result.Failure(AppointmentErrors.NotFound);

        if (appointment.Status != AppointmentStatus.Waiting)
            return Result.Failure(AppointmentErrors.CannotStart);

        if (appointment.ActualStartTime.HasValue)
            return Result.Failure(AppointmentErrors.AlreadyStarted);

        // [Phase 6] Check doctor has arrived - TODO
        // For now, we allow starting without this check

        var doctorHasActiveAppointment = await _context.Appointments.AnyAsync(a =>
            a.Id != id &&
            a.DoctorId == appointment.DoctorId &&
            a.AppointmentDate == appointment.AppointmentDate &&
            a.Status == AppointmentStatus.InProgress,
            cancellationToken);

        if (doctorHasActiveAppointment)
            return Result.Failure(AppointmentErrors.DoctorHasActiveAppointment);

        appointment.ActualStartTime = DateTime.UtcNow;
        appointment.Status = AppointmentStatus.InProgress;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> CompleteAppointmentAsync(int id, CancellationToken cancellationToken)
    {
        var appointment = await _context.Appointments.FindAsync([id], cancellationToken);
        if (appointment is null)
            return Result.Failure(AppointmentErrors.NotFound);

        if (appointment.Status != AppointmentStatus.InProgress)
            return Result.Failure(AppointmentErrors.CannotComplete);

        if (appointment.ActualEndTime.HasValue)
            return Result.Failure(AppointmentErrors.AlreadyCompleted);

        appointment.ActualEndTime = DateTime.UtcNow;
        appointment.Status = AppointmentStatus.Completed;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> CancelAppointmentAsync(int id, CancellationToken cancellationToken)
    {
        var appointment = await _context.Appointments.FindAsync([id], cancellationToken);
        if (appointment is null)
            return Result.Failure(AppointmentErrors.NotFound);

        if (appointment.Status is AppointmentStatus.Completed or AppointmentStatus.NoShow)
            return Result.Failure(AppointmentErrors.CannotCancel);

        if (appointment.Status == AppointmentStatus.Cancelled)
            return Result.Failure(AppointmentErrors.AlreadyCancelled);

        appointment.Status = AppointmentStatus.Cancelled;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    // ========== Queue Management ==========

    public async Task<Result<QueueInfo>> GetDoctorQueueAsync(int doctorId, DateOnly date, CancellationToken cancellationToken)
    {
        var doctor = await _context.Doctor
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == doctorId, cancellationToken);

        if (doctor is null)
            return Result.Failure<QueueInfo>(AppointmentErrors.DoctorNotFound);

        var current = await _context.Appointments
            .Include(a => a.Patient)
            .AsNoTracking()
            .Where(a => a.DoctorId == doctorId &&
                        a.AppointmentDate == date &&
                        a.Status == AppointmentStatus.InProgress)
            .FirstOrDefaultAsync(cancellationToken);

        var waiting = await _context.Appointments
            .Include(a => a.Patient)
            .AsNoTracking()
            .Where(a => a.DoctorId == doctorId &&
                        a.AppointmentDate == date &&
                        a.Status == AppointmentStatus.Waiting)
            .OrderBy(a => a.CheckedInAt)
            .ToListAsync(cancellationToken);

        var estimatedMinutes = waiting.Count * _consultationDurationMinutes;
        estimatedMinutes += GetRemainingMinutesForCurrent(current);

        var queueInfo = new QueueInfo(
            current,
            waiting,
            waiting.Count,
            estimatedMinutes,
            doctor
        );

        return Result.Success(queueInfo);
    }

    private int GetRemainingMinutesForCurrent(Appointment? current)
    {
        if (current is null)
            return 0;

        if (current.ActualStartTime is not DateTime started)
            return _consultationDurationMinutes;

        var elapsed = (DateTime.UtcNow - started).TotalMinutes;
        return Math.Max(0, (int)Math.Ceiling(_consultationDurationMinutes - elapsed));
    }

    private async Task<Error?> ValidateBookingAsync(
        Appointment appointment,
        int? excludeAppointmentId,
        CancellationToken cancellationToken)
    {
        var patientExists = await _context.Patients
            .AnyAsync(p => p.Id == appointment.PatientId, cancellationToken);
        if (!patientExists)
            return AppointmentErrors.PatientNotFound;

        var doctorExists = await _context.Doctor
            .AnyAsync(d => d.Id == appointment.DoctorId, cancellationToken);
        if (!doctorExists)
            return AppointmentErrors.DoctorNotFound;

        if (appointment.EndTime <= appointment.StartTime)
            return AppointmentErrors.InvalidTimeRange;

        var appointmentDateTime = appointment.AppointmentDate.ToDateTime(appointment.StartTime);
        if (appointmentDateTime < DateTime.Now)
            return AppointmentErrors.PastAppointment;

        // [Phase 5] Validate doctor schedule - TODO

        var doctorConflictQuery = _context.Appointments.Where(a =>
            a.DoctorId == appointment.DoctorId &&
            a.AppointmentDate == appointment.AppointmentDate &&
            a.Status != AppointmentStatus.Cancelled &&
            a.Status != AppointmentStatus.NoShow &&
            a.StartTime < appointment.EndTime &&
            a.EndTime > appointment.StartTime);

        if (excludeAppointmentId.HasValue)
            doctorConflictQuery = doctorConflictQuery.Where(a => a.Id != excludeAppointmentId.Value);

        if (await doctorConflictQuery.AnyAsync(cancellationToken))
            return AppointmentErrors.DoctorConflict;

        var patientConflictQuery = _context.Appointments.Where(a =>
            a.PatientId == appointment.PatientId &&
            a.AppointmentDate == appointment.AppointmentDate &&
            a.Status != AppointmentStatus.Cancelled &&
            a.Status != AppointmentStatus.NoShow &&
            a.StartTime < appointment.EndTime &&
            a.EndTime > appointment.StartTime);

        if (excludeAppointmentId.HasValue)
            patientConflictQuery = patientConflictQuery.Where(a => a.Id != excludeAppointmentId.Value);

        if (await patientConflictQuery.AnyAsync(cancellationToken))
            return AppointmentErrors.PatientConflict;

        return null;
    }
}
