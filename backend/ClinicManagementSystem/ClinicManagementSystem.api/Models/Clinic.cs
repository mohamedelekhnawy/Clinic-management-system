namespace ClinicManagementSystem.api.Models
{
    public class Clinic : AuditableEntity
    {
        public int Id { get; set; }
        public string Name_En { get; set; } = string.Empty;
        public string Name_Ar { get; set; } = string.Empty;
        public string Address_En { get; set; } = string.Empty;
        public string Address_Ar { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime OpenTime { get; set; }
        public DateTime CloseTime { get; set; }
        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    }
}
