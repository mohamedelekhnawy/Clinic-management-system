
using ClinicManagementSystem.api.Persistence;

namespace ClinicManagementSystem.api.Services
{
    public class DoctorService(ApplicationDbContext context) : IDoctorService
    {
        private readonly ApplicationDbContext _context = context;
        public async Task<IEnumerable<Doctor>> GetAllAsync(CancellationToken cancellationToken)=>
            await _context.Doctor.AsNoTracking().ToListAsync(cancellationToken);

        public async Task<Doctor?> GetAsync(int id, CancellationToken cancellationToken)=>
            await _context.Doctor.FindAsync(id,cancellationToken);

        public async Task<Doctor> AddAsync(Doctor doctor, CancellationToken cancellationToken)
        {
            await _context.Doctor.AddAsync(doctor, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return doctor;
        }
        public async Task<bool> UpdateAsync(int id, Doctor doctor, CancellationToken cancellationToken)
        {
            var currentDoctor = await GetAsync(id, cancellationToken);
            if (currentDoctor is null)
                return false;

            currentDoctor.FirstName_En = doctor.FirstName_En;
            currentDoctor.FirstName_Ar = doctor.FirstName_Ar;
            currentDoctor.LastName_En = doctor.LastName_En;
            currentDoctor.LastName_Ar = doctor.LastName_Ar;
            currentDoctor.Specialty_En = doctor.Specialty_En;
            currentDoctor.Specialty_Ar = doctor.Specialty_Ar;
            currentDoctor.Description_En = doctor.Description_En;
            currentDoctor.Description_Ar = doctor.Description_Ar;
            currentDoctor.Phone = doctor.Phone;
            currentDoctor.Email = doctor.Email;
            currentDoctor.SessionPrice = doctor.SessionPrice;
            
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
        {
            var doctor = await GetAsync(id, cancellationToken);
            if (doctor is null) 
                return false;

            _context.Remove(doctor);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
