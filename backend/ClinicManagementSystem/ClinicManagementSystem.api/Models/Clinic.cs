namespace ClinicManagementSystem.api.Models
{
    public class Clinic
    {
        public int Id { get; set; }
        public string Name { get; set; } =null!;
        public string Address { get; set; } =null!;
        public string Phone { get; set; } =null!;
        public DateTime OpenTime { get; set; }
        public DateTime CloseTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
