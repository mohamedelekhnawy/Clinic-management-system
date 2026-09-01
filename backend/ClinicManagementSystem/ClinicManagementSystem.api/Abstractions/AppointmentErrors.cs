namespace ClinicManagementSystem.api.Abstractions;

public static class AppointmentErrors
{
    // ========== Basic CRUD Errors ==========
    public static readonly Error NotFound = new(
        "Appointment.NotFound",
        "Appointment not found",
        ErrorType.NotFound);

    // ========== Foreign Key Validation Errors ==========
    public static readonly Error PatientNotFound = new(
        "Appointment.PatientNotFound",
        "The specified patient does not exist",
        ErrorType.NotFound);

    public static readonly Error DoctorNotFound = new(
        "Appointment.DoctorNotFound",
        "The specified doctor does not exist",
        ErrorType.NotFound);

    // ========== Time Validation Errors ==========
    public static readonly Error InvalidTimeRange = new(
        "Appointment.InvalidTimeRange",
        "End time must be after start time",
        ErrorType.Validation);

    public static readonly Error PastAppointment = new(
        "Appointment.PastAppointment",
        "Cannot create appointment in the past",
        ErrorType.Validation);

    // ========== Conflict Errors ==========
    public static readonly Error DoctorConflict = new(
        "Appointment.DoctorConflict",
        "Doctor has another appointment at this time",
        ErrorType.Conflict);

    public static readonly Error PatientConflict = new(
        "Appointment.PatientConflict",
        "Patient has another appointment at this time",
        ErrorType.Conflict);

    // ========== Schedule Errors (Phase 5) ==========
    public static readonly Error DoctorNotScheduled = new(
        "Appointment.DoctorNotScheduled",
        "Doctor does not have a schedule for this day",
        ErrorType.Validation);

    public static readonly Error OutsideWorkingHours = new(
        "Appointment.OutsideWorkingHours",
        "Appointment is outside doctor's working hours",
        ErrorType.Validation);

    // ========== Check-In Errors ==========
    public static readonly Error CannotCheckIn = new(
        "Appointment.CannotCheckIn",
        "Can only check in scheduled or confirmed appointments",
        ErrorType.Validation);

    public static readonly Error AlreadyCheckedIn = new(
        "Appointment.AlreadyCheckedIn",
        "Patient has already checked in",
        ErrorType.Conflict);

    // ========== Start Appointment Errors ==========
    public static readonly Error CannotStart = new(
        "Appointment.CannotStart",
        "Can only start appointments that are in Waiting status",
        ErrorType.Validation);

    public static readonly Error AlreadyStarted = new(
        "Appointment.AlreadyStarted",
        "Appointment has already started",
        ErrorType.Conflict);

    public static readonly Error DoctorHasActiveAppointment = new(
        "Appointment.DoctorHasActiveAppointment",
        "Doctor already has an appointment in progress",
        ErrorType.Conflict);

    public static readonly Error DoctorNotArrived = new(
        "Appointment.DoctorNotArrived",
        "Doctor has not arrived at the clinic yet",
        ErrorType.Validation);

    // ========== Complete Appointment Errors ==========
    public static readonly Error CannotComplete = new(
        "Appointment.CannotComplete",
        "Can only complete appointments that are in InProgress status",
        ErrorType.Validation);

    public static readonly Error AlreadyCompleted = new(
        "Appointment.AlreadyCompleted",
        "Appointment has already been completed",
        ErrorType.Conflict);

    // ========== Cancellation Errors ==========
    public static readonly Error CannotCancel = new(
        "Appointment.CannotCancel",
        "Cannot cancel completed appointments",
        ErrorType.Validation);

    public static readonly Error AlreadyCancelled = new(
        "Appointment.AlreadyCancelled",
        "Appointment has already been cancelled",
        ErrorType.Conflict);

    // ========== Update Errors ==========
    public static readonly Error CannotUpdate = new(
        "Appointment.CannotUpdate",
        "Cannot update appointments that are not in Scheduled or Confirmed status",
        ErrorType.Validation);

    // ========== Delete Errors ==========
    public static readonly Error CannotDelete = new(
        "Appointment.CannotDelete",
        "Cannot delete appointments that are not in Scheduled or Cancelled status",
        ErrorType.Validation);

    // ========== Authorization Errors ==========
    public static readonly Error Unauthorized = new(
        "Appointment.Unauthorized",
        "You are not authorized to access this appointment",
        ErrorType.Validation);
}
