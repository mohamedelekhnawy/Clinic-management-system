
namespace ClinicManagementSystem.api.Services
{
    public class ClinicService(ApplicationDbContext context) : IClinicService
    {
        private readonly ApplicationDbContext _context = context;
        
        public async Task<Result<IEnumerable<Clinic>>> GetAllAsync(CancellationToken cancellationToken)
        {
            var clinics = await _context.Clinics.AsNoTracking().ToListAsync(cancellationToken);
            return Result.Success<IEnumerable<Clinic>>(clinics);
        }

        public async Task<Result<Clinic>> GetAsync(int id, CancellationToken cancellationToken)
        {
            var clinic = await _context.Clinics.FindAsync([id], cancellationToken);
            
            return clinic is null
                ? Result.Failure<Clinic>(ClinicErrors.NotFound)
                : Result.Success(clinic);
        }

        public async Task<Result<Clinic>> AddAsync(Clinic clinic, CancellationToken cancellationToken)
        {
            await _context.Clinics.AddAsync(clinic, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success(clinic);
        }

        public async Task<Result> UpdateAsync(int id, Clinic clinic, CancellationToken cancellationToken)
        {
            var currentClinic = await _context.Clinics.FindAsync([id], cancellationToken);

            if (currentClinic is null)
                return Result.Failure(ClinicErrors.NotFound);

            currentClinic.Name_En = clinic.Name_En;
            currentClinic.Name_Ar = clinic.Name_Ar;
            currentClinic.Address_En = clinic.Address_En;
            currentClinic.Address_Ar = clinic.Address_Ar;
            currentClinic.Phone = clinic.Phone;
            currentClinic.OpenTime = clinic.OpenTime;
            currentClinic.CloseTime = clinic.CloseTime;
            
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }

        public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken)
        {
            var clinic = await _context.Clinics.FindAsync([id], cancellationToken);

            if (clinic is null)
                return Result.Failure(ClinicErrors.NotFound);

            _context.Remove(clinic);
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
