namespace ClinicManagementSystem.api.Contracts.Appointment;

public record AppointmentResponse(
    int Id,
    int PatientId,
    string PatientName_En,
    string PatientName_Ar,
    int DoctorId,
    string DoctorName_En,
    string DoctorName_Ar,
    DateOnly AppointmentDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string Status,
    DateTime? CheckedInAt,
    DateTime? ActualStartTime,
    DateTime? ActualEndTime,
    string? Notes,
    DateTime CreatedOn,
    DateTime? UpdatedOn
);
