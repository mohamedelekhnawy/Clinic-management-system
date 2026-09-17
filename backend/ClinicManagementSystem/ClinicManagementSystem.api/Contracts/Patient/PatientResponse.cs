using ClinicManagementSystem.api.Contracts.Profile;

namespace ClinicManagementSystem.api.Contracts.Patient;

public record PatientResponse(
    int Id,
    ProfileResponse Profile,
    DateOnly DateOfBirth,
    int Age,
    string Gender,
    string? Address_En,
    string? Address_Ar,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
