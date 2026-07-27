namespace ClinicManagementSystem.api.Contracts.Clinic
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
        DateTime CreatedOn,
        string? CreatedBy,
        DateTime? UpdatedOn,
        string? UpdatedBy
    );
}
