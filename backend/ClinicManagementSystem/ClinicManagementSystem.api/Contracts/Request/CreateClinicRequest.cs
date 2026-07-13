namespace ClinicManagementSystem.api.Contracts.Request
{
    public record CreateClinicRequest(
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
