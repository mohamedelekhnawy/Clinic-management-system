using ClinicManagementSystem.api.Abstractions;
using ClinicManagementSystem.api.Persistence;

namespace ClinicManagementSystem.api.Services
{
    public class DoctorService(ApplicationDbContext context) : IDoctorService
    {
        private readonly ApplicationDbContext _context = context;
        
        public async Task<Result<IEnumerable<Doctor>>> GetAllAsync(CancellationToken cancellationToken)
        {
            var doctors = await _context.Doctor.AsNoTracking().ToListAsync(cancellationToken);
            return Result.Success<IEnumerable<Doctor>>(doctors);
        }

        public async Task<Result<Doctor>> GetAsync(int id, CancellationToken cancellationToken)
        {
            var doctor = await _context.Doctor.FindAsync([id], cancellationToken);
            
            return doctor is null 
                ? Result.Failure<Doctor>(DoctorErrors.NotFound)
                : Result.Success(doctor);
        }

        public async Task<Result<Doctor>> AddAsync(Doctor doctor, CancellationToken cancellationToken)
        {
            await _context.Doctor.AddAsync(doctor, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success(doctor);
        }

        public async Task<Result> UpdateAsync(int id, Doctor doctor, CancellationToken cancellationToken)
        {
            var currentDoctor = await _context.Doctor.FindAsync([id], cancellationToken);
            if (currentDoctor is null)
                return Result.Failure(DoctorErrors.NotFound);

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
            return Result.Success();
        }

        public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken)
        {
            var doctor = await _context.Doctor.FindAsync([id], cancellationToken);
            if (doctor is null) 
                return Result.Failure(DoctorErrors.NotFound);

            _context.Remove(doctor);
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
