namespace ClinicManagementSystem.api.Services;

public interface IAssistantService
{
    Task<Result<IEnumerable<Models.Assistant>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<Models.Assistant>> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<Models.Assistant>> AddAsync(Models.Assistant assistant, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(int id, Models.Assistant assistant, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<Models.Assistant>>> GetByClinicIdAsync(int clinicId, CancellationToken cancellationToken = default);
}
