namespace ClinicManagementSystem.api.Contracts.Profile;

public record ProfileResponse(
    int Id,
    string FirstName_En,
    string FirstName_Ar,
    string LastName_En,
    string LastName_Ar,
    string Phone,
    string Email,
    bool IsActive,
    DateTime CreatedOn,
    DateTime? UpdatedOn
);
