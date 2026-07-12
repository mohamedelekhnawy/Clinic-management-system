

using ClinicManagementSystem.api.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ClinicManagementSystem.api.Services
{
    public class ClinicService : IClinicService
    {
        private readonly List<Clinic> _clinics = [
            new Clinic { Id = 1, Name = "Clinic A", Address = "123 Main St", Phone = "123-456-7890", OpenTime = DateTime.Parse("08:00"), CloseTime = DateTime.Parse("17:00"), CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
        ];

        public IEnumerable<Clinic> GetAll() => _clinics;

        public Clinic? Get(int id)=> _clinics.SingleOrDefault(c => c.Id == id);

        public Clinic Add(Clinic clinic)
        {
            clinic.Id = _clinics.Count+1;
            _clinics.Add(clinic);
            return clinic;
        }

        public bool Update(int id, Clinic clinic)
        {
            var curruntClinic = Get(id);

            if (curruntClinic is null)
                return false;   
            curruntClinic.Name = clinic.Name;
            curruntClinic.Address = clinic.Address;
            curruntClinic.Phone = clinic.Phone;
            curruntClinic.OpenTime = clinic.OpenTime;
            curruntClinic.CloseTime = clinic.CloseTime;
            curruntClinic.UpdatedAt = DateTime.Now;

            return true;
        }

        public bool Delete(int id)
        {
            var Clinic = Get(id);

            if (Clinic is null)
                return false;

            _clinics.Remove(Clinic);
            return true;
        }
    }
}
