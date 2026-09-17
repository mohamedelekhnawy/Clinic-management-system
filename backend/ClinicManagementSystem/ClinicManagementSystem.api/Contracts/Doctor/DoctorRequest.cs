using ClinicManagementSystem.api.Contracts.Profile;

namespace ClinicManagementSystem.api.Contracts.Doctor
{
    public record DoctorRequest(
        int ClinicId,
        ProfileRequest Profile,
        string Specialty_En,
        string Specialty_Ar,
        string Description_En,
        string Description_Ar,
        decimal SessionPrice
    );

}
