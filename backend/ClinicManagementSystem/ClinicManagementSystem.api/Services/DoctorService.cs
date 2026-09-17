using ClinicManagementSystem.api.Abstractions;
using ClinicManagementSystem.api.Persistence;
using Microsoft.Extensions.Caching.Hybrid;

namespace ClinicManagementSystem.api.Services
{
    public class DoctorService(ApplicationDbContext context, HybridCache cache, IProfileService profileService) : IDoctorService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly HybridCache _cache = cache;
        private readonly IProfileService _profileService = profileService;
        private const string DoctorsAllCacheKey = "doctors:all";
        private static string GetDoctorCacheKey(int id) => $"doctor:{id}";
        
        public async Task<Result<IEnumerable<Doctor>>> GetAllAsync(CancellationToken cancellationToken)
        {
            var doctors = await _cache.GetOrCreateAsync(
                DoctorsAllCacheKey,
                async cancel => await _context.Doctor
                    .Include(d => d.Profile)
                    .AsNoTracking()
                    .ToListAsync(cancel),
                new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromMinutes(10),
                    LocalCacheExpiration = TimeSpan.FromMinutes(10)
                },
                cancellationToken: cancellationToken
            );
            
            return Result.Success<IEnumerable<Doctor>>(doctors);
        }

        public async Task<Result<Doctor>> GetAsync(int id, CancellationToken cancellationToken)
        {
            var doctor = await _cache.GetOrCreateAsync(
                GetDoctorCacheKey(id),
                async cancel => await _context.Doctor
                    .Include(d => d.Profile)
                    .FirstOrDefaultAsync(d => d.Id == id, cancel),
                new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromMinutes(10),
                    LocalCacheExpiration = TimeSpan.FromMinutes(10)
                },
                cancellationToken: cancellationToken
            );
            
            return doctor is null 
                ? Result.Failure<Doctor>(DoctorErrors.NotFound)
                : Result.Success(doctor);
        }

        public async Task<Result<Doctor>> AddAsync(Doctor doctor, CancellationToken cancellationToken)
        {
            // Validate that clinic exists before adding doctor
            var clinicExists = await _context.Clinics.AnyAsync(c => c.Id == doctor.ClinicId, cancellationToken);
            if (!clinicExists)
                return Result.Failure<Doctor>(DoctorErrors.ClinicNotFound);

            // Create profile first
            var profile = new Models.Profile
            {
                FirstName_En = "", // Will be set from request in controller
                FirstName_Ar = "",
                LastName_En = "",
                LastName_Ar = "",
                Phone = "",
                Email = "",
                IsActive = true
            };
            
            // Note: Profile should be created via controller/request mapping
            // This is a placeholder - actual implementation will pass profile from controller

            try
            {
                await _context.Doctor.AddAsync(doctor, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                
                // Load profile for response
                await _context.Entry(doctor).Reference(d => d.Profile).LoadAsync(cancellationToken);
                
                // Invalidate list cache
                await _cache.RemoveAsync(DoctorsAllCacheKey, cancellationToken);
                
                return Result.Success(doctor);
            }
            catch (DbUpdateException ex)
            {
                // Profile-related errors will be caught here
                throw;
            }
        }

        public async Task<Result> UpdateAsync(int id, Doctor doctor, CancellationToken cancellationToken)
        {
            var currentDoctor = await _context.Doctor
                .Include(d => d.Profile)
                .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
                
            if (currentDoctor is null)
                return Result.Failure(DoctorErrors.NotFound);

            // Validate that clinic exists if changing clinic
            if (currentDoctor.ClinicId != doctor.ClinicId)
            {
                var clinicExists = await _context.Clinics.AnyAsync(c => c.Id == doctor.ClinicId, cancellationToken);
                if (!clinicExists)
                    return Result.Failure(DoctorErrors.ClinicNotFound);
            }

            // Update only doctor-specific fields (Profile updated separately)
            currentDoctor.Specialty_En = doctor.Specialty_En;
            currentDoctor.Specialty_Ar = doctor.Specialty_Ar;
            currentDoctor.Description_En = doctor.Description_En;
            currentDoctor.Description_Ar = doctor.Description_Ar;
            currentDoctor.SessionPrice = doctor.SessionPrice;
            currentDoctor.ClinicId = doctor.ClinicId;
            
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                
                // Invalidate both specific and list caches
                await _cache.RemoveAsync(GetDoctorCacheKey(id), cancellationToken);
                await _cache.RemoveAsync(DoctorsAllCacheKey, cancellationToken);
                
                return Result.Success();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Check if record still exists
                var exists = await _context.Doctor.AnyAsync(d => d.Id == id, cancellationToken);
                if (!exists)
                    return Result.Failure(DoctorErrors.NotFound);
                
                throw;
            }
        }

        public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken)
        {
            var doctor = await _context.Doctor
                .Include(d => d.Profile)
                .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
                
            if (doctor is null) 
                return Result.Failure(DoctorErrors.NotFound);

            try
            {
                _context.Remove(doctor);
                await _context.SaveChangesAsync(cancellationToken);
                
                // Invalidate both specific and list caches
                await _cache.RemoveAsync(GetDoctorCacheKey(id), cancellationToken);
                await _cache.RemoveAsync(DoctorsAllCacheKey, cancellationToken);
                
                return Result.Success();
            }
            catch (DbUpdateException ex)
            {
                // Check for foreign key constraint violations (e.g., doctor has appointments)
                if (ex.InnerException?.Message.Contains("REFERENCE", StringComparison.OrdinalIgnoreCase) == true ||
                    ex.InnerException?.Message.Contains("foreign key", StringComparison.OrdinalIgnoreCase) == true)
                {
                    return Result.Failure(DoctorErrors.HasDependentRecords);
                }
                
                throw;
            }
        }
    }
}
