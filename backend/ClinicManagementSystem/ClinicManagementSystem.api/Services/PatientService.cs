namespace ClinicManagementSystem.api.Services;

public class PatientService(ApplicationDbContext context) : IPatientService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<Result<IEnumerable<Models.Patient>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var patients = await _context.Patients
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedOn)
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<Models.Patient>>(patients);
    }

    public async Task<Result<Models.Patient>> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var patient = await _context.Patients.FindAsync([id], cancellationToken);

        return patient is null
            ? Result.Failure<Models.Patient>(PatientErrors.NotFound)
            : Result.Success(patient);
    }

    public async Task<Result<Models.Patient>> AddAsync(Models.Patient patient, CancellationToken cancellationToken = default)
    {
        // Validate date of birth
        if (patient.DateOfBirth >= DateOnly.FromDateTime(DateTime.Today))
            return Result.Failure<Models.Patient>(PatientErrors.InvalidDateOfBirth);

        try
        {
            await _context.Patients.AddAsync(patient, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success(patient);
        }
        catch (DbUpdateException ex)
        {
            // Check for duplicate constraint violations
            if (ex.InnerException?.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) == true ||
                ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true)
            {
                if (ex.InnerException?.Message.Contains("Email", StringComparison.OrdinalIgnoreCase) == true)
                    return Result.Failure<Models.Patient>(PatientErrors.DuplicateEmail);

                if (ex.InnerException?.Message.Contains("Phone", StringComparison.OrdinalIgnoreCase) == true)
                    return Result.Failure<Models.Patient>(PatientErrors.DuplicatePhone);
            }

            // Let other database errors bubble up to global handler
            throw;
        }
    }

    public async Task<Result> UpdateAsync(int id, Models.Patient patient, CancellationToken cancellationToken = default)
    {
        var currentPatient = await _context.Patients.FindAsync([id], cancellationToken);
        if (currentPatient is null)
            return Result.Failure(PatientErrors.NotFound);

        // Validate date of birth
        if (patient.DateOfBirth >= DateOnly.FromDateTime(DateTime.Today))
            return Result.Failure(PatientErrors.InvalidDateOfBirth);

        currentPatient.FirstName_En = patient.FirstName_En;
        currentPatient.FirstName_Ar = patient.FirstName_Ar;
        currentPatient.LastName_En = patient.LastName_En;
        currentPatient.LastName_Ar = patient.LastName_Ar;
        currentPatient.DateOfBirth = patient.DateOfBirth;
        currentPatient.Gender = patient.Gender;
        currentPatient.Phone = patient.Phone;
        currentPatient.Email = patient.Email;
        currentPatient.Address_En = patient.Address_En;
        currentPatient.Address_Ar = patient.Address_Ar;
        currentPatient.EmergencyContactName = patient.EmergencyContactName;
        currentPatient.EmergencyContactPhone = patient.EmergencyContactPhone;
        currentPatient.Notes = patient.Notes;
        currentPatient.IsActive = patient.IsActive;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Check if record still exists
            var exists = await _context.Patients.AnyAsync(p => p.Id == id, cancellationToken);
            if (!exists)
                return Result.Failure(PatientErrors.NotFound);

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
                    return Result.Failure(PatientErrors.DuplicateEmail);

                if (ex.InnerException?.Message.Contains("Phone", StringComparison.OrdinalIgnoreCase) == true)
                    return Result.Failure(PatientErrors.DuplicatePhone);
            }

            throw;
        }
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var patient = await _context.Patients.FindAsync([id], cancellationToken);
        if (patient is null)
            return Result.Failure(PatientErrors.NotFound);

        try
        {
            _context.Remove(patient);
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            // Check for foreign key constraint violations (e.g., patient has appointments)
            if (ex.InnerException?.Message.Contains("REFERENCE", StringComparison.OrdinalIgnoreCase) == true ||
                ex.InnerException?.Message.Contains("foreign key", StringComparison.OrdinalIgnoreCase) == true)
            {
                return Result.Failure(PatientErrors.HasDependentRecords);
            }

            throw;
        }
    }

    public async Task<Result<IEnumerable<Models.Patient>>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllAsync(cancellationToken);

        var patients = await _context.Patients
            .AsNoTracking()
            .Where(p =>
                p.FirstName_En.Contains(searchTerm) ||
                p.FirstName_Ar.Contains(searchTerm) ||
                p.LastName_En.Contains(searchTerm) ||
                p.LastName_Ar.Contains(searchTerm) ||
                p.Phone.Contains(searchTerm) ||
                (p.Email != null && p.Email.Contains(searchTerm)))
            .OrderByDescending(p => p.CreatedOn)
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<Models.Patient>>(patients);
    }
}
