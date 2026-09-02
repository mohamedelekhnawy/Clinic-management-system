namespace ClinicManagementSystem.api.Contracts.Patient;

public record PatientRequest(
    string FirstName_En,
    string FirstName_Ar,
    string LastName_En,
    string LastName_Ar,
    DateOnly DateOfBirth,
    int Gender,
    string Phone,
    string? Email,
    string? Address_En,
    string? Address_Ar,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    string? Notes,
    bool IsActive
);
