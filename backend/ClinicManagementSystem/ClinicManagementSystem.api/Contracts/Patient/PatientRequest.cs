using ClinicManagementSystem.api.Contracts.Profile;

namespace ClinicManagementSystem.api.Contracts.Patient;

public record PatientRequest(
    ProfileRequest Profile,
    DateOnly DateOfBirth,
    int Gender,
    string? Address_En,
    string? Address_Ar,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    string? Notes
);
