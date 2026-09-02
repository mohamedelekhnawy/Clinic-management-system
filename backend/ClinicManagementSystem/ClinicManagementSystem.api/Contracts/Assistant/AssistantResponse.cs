namespace ClinicManagementSystem.api.Contracts.Assistant;

public record AssistantResponse(
    int Id,
    int ClinicId,
    string ClinicName_En,
    string ClinicName_Ar,
    string FirstName_En,
    string FirstName_Ar,
    string LastName_En,
    string LastName_Ar,
    string Phone,
    string Email,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
