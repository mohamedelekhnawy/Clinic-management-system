namespace ClinicManagementSystem.api.Contracts.Responce
{
    public record ClinicResponse(
        int Id,
        string Name_En,
        string Name_Ar,
        string Address_En,
        string Address_Ar,
        string Phone,
        DateTime OpenTime,
        DateTime CloseTime,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );
}
