namespace ClinicManagementSystem.api.Services
{
    public interface IClinicService
    {
        IEnumerable<Clinic> GetAll();
        Clinic? Get(int id);
        Clinic Add(Clinic clinic);
        bool Update(int id, Clinic clinic);
        bool Delete(int id);
    }
}
