namespace ClinicManagementSystem.api.Models
{
    public class Doctor : AuditableEntity
    {
        public int Id { get; set; }
        public int ClinicId { get; set; }
        public string FirstName_En { get; set; } = string.Empty;
        public string FirstName_Ar { get; set; } = string.Empty;
        public string LastName_En { get; set; } = string.Empty;
        public string LastName_Ar { get; set; } = string.Empty;
        public string Specialty_En { get; set; } = string.Empty;
        public string Specialty_Ar { get; set; } = string.Empty;
        public string Description_En { get; set; } = string.Empty;
        public string Description_Ar { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal SessionPrice { get; set; }

        public Clinic clinic { get; set; } = null!;
    }
}
