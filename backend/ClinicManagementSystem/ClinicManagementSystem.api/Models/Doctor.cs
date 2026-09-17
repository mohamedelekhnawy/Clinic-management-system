namespace ClinicManagementSystem.api.Models
{
    public class Doctor : AuditableEntity
    {
        public int Id { get; set; }
        public int ClinicId { get; set; }
        public int ProfileId { get; set; }
        public string Specialty_En { get; set; } = string.Empty;
        public string Specialty_Ar { get; set; } = string.Empty;
        public string Description_En { get; set; } = string.Empty;
        public string Description_Ar { get; set; } = string.Empty;
        public decimal SessionPrice { get; set; }

        // Navigation properties
        public Clinic clinic { get; set; } = null!;
        public Profile Profile { get; set; } = null!;
    }
}
