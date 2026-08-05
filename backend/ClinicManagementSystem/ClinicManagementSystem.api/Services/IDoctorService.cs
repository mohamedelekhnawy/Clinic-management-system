using ClinicManagementSystem.api.Abstractions;

namespace ClinicManagementSystem.api.Services
{
    public interface IDoctorService
    {
        Task<Result<IEnumerable<Doctor>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<Doctor>> GetAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<Doctor>> AddAsync(Doctor doctor, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(int id, Doctor doctor, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
