using ClinicManagementSystem.api.Persistence;
using System.Threading.Tasks;

namespace ClinicManagementSystem.api.Services
{
    public class ClinicService(ApplicationDbContext context):IClinicService
    {
        private readonly ApplicationDbContext _context= context;
        public async Task<IEnumerable<Clinic>> GetAllAsync(CancellationToken cancellationToken) => 
            await _context.Clinics.AsNoTracking().ToListAsync(cancellationToken);

        public async Task<Clinic?> GetAsync(int id, CancellationToken cancellationToken)=> 
            await _context.Clinics.FindAsync(id,cancellationToken);

        public async Task<Clinic> AddAsync(Clinic clinic, CancellationToken cancellationToken)
        {
            await _context.Clinics.AddAsync(clinic, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return clinic;
        }

        public async Task<bool> UpdateAsync(int id, Clinic clinic,CancellationToken cancellationToken)
        {
            var curruntClinic = await GetAsync(id,cancellationToken);

            if (curruntClinic is null)
                return false;

            curruntClinic.Name_En = clinic.Name_En;
            curruntClinic.Name_Ar = clinic.Name_Ar;
            curruntClinic.Address_En = clinic.Address_En;
            curruntClinic.Address_Ar = clinic.Address_Ar;
            curruntClinic.Phone = clinic.Phone;
            curruntClinic.OpenTime = clinic.OpenTime;
            curruntClinic.CloseTime = clinic.CloseTime;
            
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<bool> DeleteAsync(int id,CancellationToken cancellationToken)
        {
            var Clinic =await GetAsync(id,cancellationToken);

            if (Clinic is null)
                return false;

            _context.Remove(Clinic);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
