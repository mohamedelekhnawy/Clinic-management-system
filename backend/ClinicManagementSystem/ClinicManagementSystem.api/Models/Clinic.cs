namespace ClinicManagementSystem.api.Models
{
    public class Clinic
    {
        public int Id { get; set; }
        public string Name_En { get; set; } =null!;
        public string Name_Ar { get; set; } = null!;
        public string Address_En { get; set; } =null!;
        public string Address_Ar { get; set; } =null!;
        public string Phone { get; set; } =null!;
        public DateTime OpenTime { get; set; }
        public DateTime CloseTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    }
}
