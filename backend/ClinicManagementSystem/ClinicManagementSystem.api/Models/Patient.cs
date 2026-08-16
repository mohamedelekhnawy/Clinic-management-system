
using System.Reflection;

namespace ClinicManagementSystem.api.Models
{
    public class Patient : AuditableEntity
    {
        public int Id { get; set; }

        public string FirstName_En { get; set; } = string.Empty;
        public string FirstName_Ar { get; set; } = string.Empty;

        public string LastName_En { get; set; } = string.Empty;
        public string LastName_Ar { get; set; } = string.Empty;

        public DateOnly DateOfBirth { get; set; }

        public Gender Gender { get; set; }

        public string Phone { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string? Address_En { get; set; }

        public string? Address_Ar { get; set; }

        public string? EmergencyContactName { get; set; }

        public string? EmergencyContactPhone { get; set; }

        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
