using ClinicManagementSystem.api.Abstractions;

namespace ClinicManagementSystem.api.Services
{
    public interface IClinicService
    {
        Task<Result<IEnumerable<Clinic>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<Clinic>> GetAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<Clinic>> AddAsync(Clinic clinic, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(int id, Clinic clinic, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
