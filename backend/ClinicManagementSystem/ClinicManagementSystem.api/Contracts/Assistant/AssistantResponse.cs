using ClinicManagementSystem.api.Contracts.Profile;

namespace ClinicManagementSystem.api.Contracts.Assistant;

public record AssistantResponse(
    int Id,
    int ClinicId,
    string ClinicName_En,
    string ClinicName_Ar,
    ProfileResponse Profile,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
