namespace ClinicManagementSystem.api.Services;

public interface IPatientService
{
    Task<Result<IEnumerable<Models.Patient>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<Models.Patient>> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<Models.Patient>> AddAsync(Models.Patient patient, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(int id, Models.Patient patient, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<Models.Patient>>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
}
