namespace ClinicManagementSystem.api.Services;

public interface IProfileService
{
    Task<Result<Models.Profile>> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<Models.Profile>> AddAsync(Models.Profile profile, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(int id, Models.Profile profile, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
