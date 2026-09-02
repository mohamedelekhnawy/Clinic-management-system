namespace ClinicManagementSystem.api.Contracts.Appointment;

public record QueueResponse(
    int DoctorId,
    string DoctorName_En,
    string DoctorName_Ar,
    QueueItemResponse? CurrentPatient,
    List<QueueItemResponse> WaitingPatients,
    int TotalWaiting,
    int EstimatedWaitingMinutes
);
