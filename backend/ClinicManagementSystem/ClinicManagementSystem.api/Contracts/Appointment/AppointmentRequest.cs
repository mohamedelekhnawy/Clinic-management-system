namespace ClinicManagementSystem.api.Contracts.Appointment;

public record AppointmentRequest(
    int PatientId,
    int DoctorId,
    DateOnly AppointmentDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string? Notes
);
