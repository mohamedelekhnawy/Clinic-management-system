namespace ClinicManagementSystem.api.Contracts.Profile;

public record UpdateProfileRequest(
    string FirstName_En,
    string FirstName_Ar,
    string LastName_En,
    string LastName_Ar,
    string Phone,
    string Email
);
