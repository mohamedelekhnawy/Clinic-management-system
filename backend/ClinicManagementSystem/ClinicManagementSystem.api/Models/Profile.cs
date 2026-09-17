namespace ClinicManagementSystem.api.Models
{
    public class Profile : AuditableEntity
    {
        public int Id { get; set; }
        public string? ApplicationUserId { get; set; }
        public string FirstName_En { get; set; } = string.Empty;
        public string FirstName_Ar { get; set; } = string.Empty;
        public string LastName_En { get; set; } = string.Empty;
        public string LastName_Ar { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public ApplicationUser? ApplicationUser { get; set; }
    }
}
