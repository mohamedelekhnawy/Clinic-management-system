namespace ClinicManagementSystem.api.Contracts.Request
{
    public record DoctorRequest(
        int ClinicId,
        string FirstName_En,
        string FirstName_Ar,
        string LastName_En,
        string LastName_Ar,
        string Specialty_En,
        string Specialty_Ar,
        string Description_En,
        string Description_Ar,
        string Phone,
        string Email,
        decimal SessionPrice,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );

}
