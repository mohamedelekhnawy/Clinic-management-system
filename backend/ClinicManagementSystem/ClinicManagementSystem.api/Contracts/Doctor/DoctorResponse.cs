using ClinicManagementSystem.api.Contracts.Profile;

namespace ClinicManagementSystem.api.Contracts.Doctor
{
    public record DoctorResponse(
        int Id,
        int ClinicId,
        ProfileResponse Profile,
        string Specialty_En,
        string Specialty_Ar,
        string Description_En,
        string Description_Ar,
        decimal SessionPrice,
        DateTime CreatedOn,
        string? CreatedBy,
        DateTime? UpdatedOn,
        string? UpdatedBy
    );

}
