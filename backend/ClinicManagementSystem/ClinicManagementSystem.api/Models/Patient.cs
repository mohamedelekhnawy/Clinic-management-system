using System.Reflection;

namespace ClinicManagementSystem.api.Models
{
    public class Patient : AuditableEntity
    {
        public int Id { get; set; }
        public int ProfileId { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public Gender Gender { get; set; }
        public string? Address_En { get; set; }
        public string? Address_Ar { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? Notes { get; set; }

        // Navigation properties
        public Profile Profile { get; set; } = null!;
    }
}
