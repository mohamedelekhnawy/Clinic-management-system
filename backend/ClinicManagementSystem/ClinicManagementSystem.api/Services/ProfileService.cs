namespace ClinicManagementSystem.api.Services;

public class ProfileService(ApplicationDbContext context) : IProfileService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<Result<Models.Profile>> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var profile = await _context.Profiles.FindAsync([id], cancellationToken);

        return profile is null
            ? Result.Failure<Models.Profile>(ProfileErrors.NotFound)
            : Result.Success(profile);
    }

    public async Task<Result<Models.Profile>> AddAsync(Models.Profile profile, CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Profiles.AddAsync(profile, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success(profile);
        }
        catch (DbUpdateException ex)
        {
            // Check for duplicate constraint violations
            if (ex.InnerException?.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) == true ||
                ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true)
            {
                if (ex.InnerException?.Message.Contains("Email", StringComparison.OrdinalIgnoreCase) == true)
                    return Result.Failure<Models.Profile>(ProfileErrors.DuplicateEmail);

                if (ex.InnerException?.Message.Contains("Phone", StringComparison.OrdinalIgnoreCase) == true)
                    return Result.Failure<Models.Profile>(ProfileErrors.DuplicatePhone);
            }

            // Let other database errors bubble up to global handler
            throw;
        }
    }

    public async Task<Result> UpdateAsync(int id, Models.Profile profile, CancellationToken cancellationToken = default)
    {
        var currentProfile = await _context.Profiles.FindAsync([id], cancellationToken);
        if (currentProfile is null)
            return Result.Failure(ProfileErrors.NotFound);

        currentProfile.FirstName_En = profile.FirstName_En;
        currentProfile.FirstName_Ar = profile.FirstName_Ar;
        currentProfile.LastName_En = profile.LastName_En;
        currentProfile.LastName_Ar = profile.LastName_Ar;
        currentProfile.Phone = profile.Phone;
        currentProfile.Email = profile.Email;
        currentProfile.IsActive = profile.IsActive;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Check if record still exists
            var exists = await _context.Profiles.AnyAsync(p => p.Id == id, cancellationToken);
            if (!exists)
                return Result.Failure(ProfileErrors.NotFound);

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
                    return Result.Failure(ProfileErrors.DuplicateEmail);

                if (ex.InnerException?.Message.Contains("Phone", StringComparison.OrdinalIgnoreCase) == true)
                    return Result.Failure(ProfileErrors.DuplicatePhone);
            }

            throw;
        }
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var profile = await _context.Profiles.FindAsync([id], cancellationToken);
        if (profile is null)
            return Result.Failure(ProfileErrors.NotFound);

        try
        {
            _context.Remove(profile);
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            // Check for foreign key constraint violations
            if (ex.InnerException?.Message.Contains("REFERENCE", StringComparison.OrdinalIgnoreCase) == true ||
                ex.InnerException?.Message.Contains("foreign key", StringComparison.OrdinalIgnoreCase) == true)
            {
                return Result.Failure(ProfileErrors.HasDependentRecords);
            }

            throw;
        }
    }
}
