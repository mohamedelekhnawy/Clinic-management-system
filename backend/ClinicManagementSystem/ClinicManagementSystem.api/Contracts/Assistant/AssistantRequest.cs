using ClinicManagementSystem.api.Contracts.Profile;

namespace ClinicManagementSystem.api.Contracts.Assistant;

public record AssistantRequest(
    int ClinicId,
    ProfileRequest Profile
);
