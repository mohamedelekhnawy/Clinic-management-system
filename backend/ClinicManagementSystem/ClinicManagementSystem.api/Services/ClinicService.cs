using Microsoft.Extensions.Caching.Hybrid;

namespace ClinicManagementSystem.api.Services
{
    public class ClinicService(ApplicationDbContext context, HybridCache cache) : IClinicService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly HybridCache _cache = cache;
        private const string ClinicsAllCacheKey = "clinics:all";
        private static string GetClinicCacheKey(int id) => $"clinic:{id}";
        
        public async Task<Result<IEnumerable<Clinic>>> GetAllAsync(CancellationToken cancellationToken)
        {
            var clinics = await _cache.GetOrCreateAsync(
                ClinicsAllCacheKey,
                async cancel => await _context.Clinics.AsNoTracking().ToListAsync(cancel),
                new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromMinutes(10),
                    LocalCacheExpiration = TimeSpan.FromMinutes(10)
                },
                cancellationToken: cancellationToken
            );
            
            return Result.Success<IEnumerable<Clinic>>(clinics);
        }

        public async Task<Result<Clinic>> GetAsync(int id, CancellationToken cancellationToken)
        {
            var clinic = await _cache.GetOrCreateAsync(
                GetClinicCacheKey(id),
                async cancel => await _context.Clinics.FindAsync([id], cancel),
                new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromMinutes(10),
                    LocalCacheExpiration = TimeSpan.FromMinutes(10)
                },
                cancellationToken: cancellationToken
            );
            
            return clinic is null
                ? Result.Failure<Clinic>(ClinicErrors.NotFound)
                : Result.Success(clinic);
        }

        public async Task<Result<Clinic>> AddAsync(Clinic clinic, CancellationToken cancellationToken)
        {
            try
            {
                await _context.Clinics.AddAsync(clinic, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                
                // Invalidate list cache
                await _cache.RemoveAsync(ClinicsAllCacheKey, cancellationToken);
                
                return Result.Success(clinic);
            }
            catch (DbUpdateException ex)
            {
                // Check for duplicate constraint violations
                if (ex.InnerException?.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) == true ||
                    ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true)
                {
                    return Result.Failure<Clinic>(ClinicErrors.DuplicateName);
                }
                
                // Let other database errors bubble up to global handler
                throw;
            }
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
            
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                
                // Invalidate both specific and list caches
                await _cache.RemoveAsync(GetClinicCacheKey(id), cancellationToken);
                await _cache.RemoveAsync(ClinicsAllCacheKey, cancellationToken);
                
                return Result.Success();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Check if record still exists
                var exists = await _context.Clinics.AnyAsync(c => c.Id == id, cancellationToken);
                if (!exists)
                    return Result.Failure(ClinicErrors.NotFound);
                
                // Let concurrency exception bubble up to global handler
                throw;
            }
            catch (DbUpdateException ex)
            {
                // Check for duplicate constraint violations
                if (ex.InnerException?.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) == true ||
                    ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true)
                {
                    return Result.Failure(ClinicErrors.DuplicateName);
                }
                
                throw;
            }
        }

        public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken)
        {
            var clinic = await _context.Clinics.FindAsync([id], cancellationToken);

            if (clinic is null)
                return Result.Failure(ClinicErrors.NotFound);

            try
            {
                _context.Remove(clinic);
                await _context.SaveChangesAsync(cancellationToken);
                
                // Invalidate both specific and list caches
                await _cache.RemoveAsync(GetClinicCacheKey(id), cancellationToken);
                await _cache.RemoveAsync(ClinicsAllCacheKey, cancellationToken);
                
                return Result.Success();
            }
            catch (DbUpdateException ex)
            {
                // Check for foreign key constraint violations (e.g., clinic has doctors)
                if (ex.InnerException?.Message.Contains("REFERENCE", StringComparison.OrdinalIgnoreCase) == true ||
                    ex.InnerException?.Message.Contains("foreign key", StringComparison.OrdinalIgnoreCase) == true)
                {
                    return Result.Failure(ClinicErrors.HasDependentDoctors);
                }
                
                throw;
            }
        }
    }
}
