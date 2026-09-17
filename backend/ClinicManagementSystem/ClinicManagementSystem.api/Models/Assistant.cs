namespace ClinicManagementSystem.api.Models
{
    public class Assistant : AuditableEntity
    {
        public int Id { get; set; }
        public int ClinicId { get; set; }
        public int ProfileId { get; set; }
        
        // Navigation properties
        public Clinic Clinic { get; set; } = null!;
        public Profile Profile { get; set; } = null!;
    }
}
