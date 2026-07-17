namespace ClinicManagementSystem.api.Models
{
    public class Doctor
    {
        public int Id { get; set; }
        public int ClinicId { get; set; }
        public string FirstName_En { get; set; }
        public string FirstName_Ar { get; set; }
        public string LastName_En { get; set; } 
        public string LastName_Ar { get; set; } 
        public string Specialty_En { get; set; } 
        public string Specialty_Ar { get; set; } 
        public string Description_En { get; set; }
        public string Description_Ar { get; set; } 
        public string Phone { get; set; }
        public string Email { get; set; } 
        public decimal SessionPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set;}

        public Clinic clinic { get; set; }=null !;

    }
}
