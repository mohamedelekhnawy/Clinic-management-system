namespace ClinicManagementSystem.api.Contracts.Authentication
{
    public record RegisterRequest(
        string FirstName_EN,
        string? FirstName_AR,
        string LastName_EN,
        string? LastName_AR,
        string Email,
        string Password
    );
}
