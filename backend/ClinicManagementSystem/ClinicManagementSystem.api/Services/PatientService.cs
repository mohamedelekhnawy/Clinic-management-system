namespace ClinicManagementSystem.api.Services;

public class PatientService(ApplicationDbContext context, IProfileService profileService) : IPatientService
{
    private readonly ApplicationDbContext _context = context;
    private readonly IProfileService _profileService = profileService;

    public async Task<Result<IEnumerable<Models.Patient>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var patients = await _context.Patients
            .Include(p => p.Profile)
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedOn)
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<Models.Patient>>(patients);
    }

    public async Task<Result<Models.Patient>> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var patient = await _context.Patients
            .Include(p => p.Profile)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        return patient is null
            ? Result.Failure<Models.Patient>(PatientErrors.NotFound)
            : Result.Success(patient);
    }

    public async Task<Result<Models.Patient>> AddAsync(Models.Patient patient, CancellationToken cancellationToken = default)
    {
        // Validate date of birth
        if (patient.DateOfBirth >= DateOnly.FromDateTime(DateTime.Today))
            return Result.Failure<Models.Patient>(PatientErrors.InvalidDateOfBirth);

        // Note: Profile should be created and assigned from controller
        // This assumes ProfileId is already set on patient entity

        try
        {
            await _context.Patients.AddAsync(patient, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            
            // Load profile for response
            await _context.Entry(patient).Reference(p => p.Profile).LoadAsync(cancellationToken);
            
            return Result.Success(patient);
        }
        catch (DbUpdateException)
        {
            // Profile-related errors will be caught here
            throw;
        }
    }

    public async Task<Result> UpdateAsync(int id, Models.Patient patient, CancellationToken cancellationToken = default)
    {
        var currentPatient = await _context.Patients
            .Include(p => p.Profile)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
            
        if (currentPatient is null)
            return Result.Failure(PatientErrors.NotFound);

        // Validate date of birth
        if (patient.DateOfBirth >= DateOnly.FromDateTime(DateTime.Today))
            return Result.Failure(PatientErrors.InvalidDateOfBirth);

        // Update only patient-specific fields (Profile updated separately)
        currentPatient.DateOfBirth = patient.DateOfBirth;
        currentPatient.Gender = patient.Gender;
        currentPatient.Address_En = patient.Address_En;
        currentPatient.Address_Ar = patient.Address_Ar;
        currentPatient.EmergencyContactName = patient.EmergencyContactName;
        currentPatient.EmergencyContactPhone = patient.EmergencyContactPhone;
        currentPatient.Notes = patient.Notes;

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
            .Include(p => p.Profile)
            .AsNoTracking()
            .Where(p =>
                p.Profile.FirstName_En.Contains(searchTerm) ||
                p.Profile.FirstName_Ar.Contains(searchTerm) ||
                p.Profile.LastName_En.Contains(searchTerm) ||
                p.Profile.LastName_Ar.Contains(searchTerm) ||
                p.Profile.Phone.Contains(searchTerm) ||
                (p.Profile.Email != null && p.Profile.Email.Contains(searchTerm)))
            .OrderByDescending(p => p.CreatedOn)
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<Models.Patient>>(patients);
    }
}
