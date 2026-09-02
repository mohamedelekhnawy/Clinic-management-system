using Microsoft.Extensions.Caching.Hybrid;

namespace ClinicManagementSystem.api.Services;

public class AssistantService(ApplicationDbContext context, HybridCache cache) : IAssistantService
{
    private readonly ApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private const string AssistantsAllCacheKey = "assistants:all";
    private static string GetAssistantCacheKey(int id) => $"assistant:{id}";
    private static string GetAssistantsByClinicCacheKey(int clinicId) => $"assistants:clinic:{clinicId}";

    public async Task<Result<IEnumerable<Models.Assistant>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var assistants = await _cache.GetOrCreateAsync(
            AssistantsAllCacheKey,
            async cancel => await _context.Assistants
                .Include(a => a.Clinic)
                .AsNoTracking()
                .OrderByDescending(a => a.CreatedOn)
                .ToListAsync(cancel),
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(10),
                LocalCacheExpiration = TimeSpan.FromMinutes(10)
            },
            cancellationToken: cancellationToken
        );

        return Result.Success<IEnumerable<Models.Assistant>>(assistants);
    }

    public async Task<Result<Models.Assistant>> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var assistant = await _cache.GetOrCreateAsync(
            GetAssistantCacheKey(id),
            async cancel => await _context.Assistants
                .Include(a => a.Clinic)
                .FirstOrDefaultAsync(a => a.Id == id, cancel),
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(10),
                LocalCacheExpiration = TimeSpan.FromMinutes(10)
            },
            cancellationToken: cancellationToken
        );

        return assistant is null
            ? Result.Failure<Models.Assistant>(AssistantErrors.NotFound)
            : Result.Success(assistant);
    }

    public async Task<Result<Models.Assistant>> AddAsync(Models.Assistant assistant, CancellationToken cancellationToken = default)
    {
        // Validate that clinic exists before adding assistant
        var clinicExists = await _context.Clinics.AnyAsync(c => c.Id == assistant.ClinicId, cancellationToken);
        if (!clinicExists)
            return Result.Failure<Models.Assistant>(AssistantErrors.ClinicNotFound);

        try
        {
            await _context.Assistants.AddAsync(assistant, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // Load clinic for response
            await _context.Entry(assistant).Reference(a => a.Clinic).LoadAsync(cancellationToken);

            // Invalidate caches
            await _cache.RemoveAsync(AssistantsAllCacheKey, cancellationToken);
            await _cache.RemoveAsync(GetAssistantsByClinicCacheKey(assistant.ClinicId), cancellationToken);

            return Result.Success(assistant);
        }
        catch (DbUpdateException ex)
        {
            // Check for duplicate email/phone constraint violations
            if (ex.InnerException?.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) == true ||
                ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true)
            {
                if (ex.InnerException?.Message.Contains("Email", StringComparison.OrdinalIgnoreCase) == true)
                    return Result.Failure<Models.Assistant>(AssistantErrors.DuplicateEmail);

                if (ex.InnerException?.Message.Contains("Phone", StringComparison.OrdinalIgnoreCase) == true)
                    return Result.Failure<Models.Assistant>(AssistantErrors.DuplicatePhone);
            }

            // Let other database errors bubble up to global handler
            throw;
        }
    }

    public async Task<Result> UpdateAsync(int id, Models.Assistant assistant, CancellationToken cancellationToken = default)
    {
        var currentAssistant = await _context.Assistants.FindAsync([id], cancellationToken);
        if (currentAssistant is null)
            return Result.Failure(AssistantErrors.NotFound);

        // Store old clinic ID for cache invalidation
        var oldClinicId = currentAssistant.ClinicId;

        // Validate that clinic exists if changing clinic
        if (currentAssistant.ClinicId != assistant.ClinicId)
        {
            var clinicExists = await _context.Clinics.AnyAsync(c => c.Id == assistant.ClinicId, cancellationToken);
            if (!clinicExists)
                return Result.Failure(AssistantErrors.ClinicNotFound);
        }

        currentAssistant.ClinicId = assistant.ClinicId;
        currentAssistant.FirstName_En = assistant.FirstName_En;
        currentAssistant.FirstName_Ar = assistant.FirstName_Ar;
        currentAssistant.LastName_En = assistant.LastName_En;
        currentAssistant.LastName_Ar = assistant.LastName_Ar;
        currentAssistant.Phone = assistant.Phone;
        currentAssistant.Email = assistant.Email;
        currentAssistant.IsActive = assistant.IsActive;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            
            // Invalidate caches
            await _cache.RemoveAsync(GetAssistantCacheKey(id), cancellationToken);
            await _cache.RemoveAsync(AssistantsAllCacheKey, cancellationToken);
            await _cache.RemoveAsync(GetAssistantsByClinicCacheKey(oldClinicId), cancellationToken);
            
            // If clinic changed, invalidate new clinic cache too
            if (oldClinicId != assistant.ClinicId)
            {
                await _cache.RemoveAsync(GetAssistantsByClinicCacheKey(assistant.ClinicId), cancellationToken);
            }
            
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Check if record still exists
            var exists = await _context.Assistants.AnyAsync(a => a.Id == id, cancellationToken);
            if (!exists)
                return Result.Failure(AssistantErrors.NotFound);

            // Let concurrency exception bubble up to global handler
            throw;
        }
        catch (DbUpdateException ex)
        {
            // Check for duplicate constraint violations
            if (ex.InnerException?.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) == true ||
                ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true)
            {
                if (ex.InnerException?.Message.Contains("Email", StringComparison.OrdinalIgnoreCase) == true)
                    return Result.Failure(AssistantErrors.DuplicateEmail);

                if (ex.InnerException?.Message.Contains("Phone", StringComparison.OrdinalIgnoreCase) == true)
                    return Result.Failure(AssistantErrors.DuplicatePhone);
            }

            throw;
        }
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var assistant = await _context.Assistants.FindAsync([id], cancellationToken);
        if (assistant is null)
            return Result.Failure(AssistantErrors.NotFound);

        var clinicId = assistant.ClinicId;

        try
        {
            _context.Remove(assistant);
            await _context.SaveChangesAsync(cancellationToken);
            
            // Invalidate caches
            await _cache.RemoveAsync(GetAssistantCacheKey(id), cancellationToken);
            await _cache.RemoveAsync(AssistantsAllCacheKey, cancellationToken);
            await _cache.RemoveAsync(GetAssistantsByClinicCacheKey(clinicId), cancellationToken);
            
            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            // Check for foreign key constraint violations (e.g., assistant has related records)
            if (ex.InnerException?.Message.Contains("REFERENCE", StringComparison.OrdinalIgnoreCase) == true ||
                ex.InnerException?.Message.Contains("foreign key", StringComparison.OrdinalIgnoreCase) == true)
            {
                return Result.Failure(AssistantErrors.HasDependentRecords);
            }

            throw;
        }
    }

    public async Task<Result<IEnumerable<Models.Assistant>>> GetByClinicIdAsync(int clinicId, CancellationToken cancellationToken = default)
    {
        var assistants = await _cache.GetOrCreateAsync(
            GetAssistantsByClinicCacheKey(clinicId),
            async cancel => await _context.Assistants
                .Include(a => a.Clinic)
                .AsNoTracking()
                .Where(a => a.ClinicId == clinicId)
                .OrderByDescending(a => a.CreatedOn)
                .ToListAsync(cancel),
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(10),
                LocalCacheExpiration = TimeSpan.FromMinutes(10)
            },
            cancellationToken: cancellationToken
        );

        return Result.Success<IEnumerable<Models.Assistant>>(assistants);
    }
}
