namespace ClinicManagementSystem.api.Contracts.Responce
{
    public record ClinicResponse(
        string Name,
        string Address,
        string Phone,
        DateTime OpenTime,
        DateTime CloseTime,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );
}
