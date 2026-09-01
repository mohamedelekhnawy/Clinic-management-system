namespace ClinicManagementSystem.api.Contracts.Appointment;

public record QueueItemResponse(
    int AppointmentId,
    int PatientId,
    string PatientName_En,
    string PatientName_Ar,
    TimeOnly ScheduledTime,
    DateTime CheckedInAt,
    int PositionInQueue
);
