namespace ClinicManagementSystem.api.Contracts.Assistant;

public record AssistantRequest(
    int ClinicId,
    string FirstName_En,
    string FirstName_Ar,
    string LastName_En,
    string LastName_Ar,
    string Phone,
    string Email,
    bool IsActive
);
