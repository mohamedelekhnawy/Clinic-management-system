namespace ClinicManagementSystem.api.Contracts.Request
{
    public record CreateClinicRequest(
        int Id,
        string Name,
        string Address,
        string Phone,
        DateTime OpenTime,
        DateTime CloseTime,
        DateTime CreatedAt,
        DateTime UpdatedAt
        );
}
