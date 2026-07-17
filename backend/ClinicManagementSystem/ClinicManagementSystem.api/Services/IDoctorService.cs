namespace ClinicManagementSystem.api.Services
{
    public interface IDoctorService
    {
        Task<IEnumerable<Doctor>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Doctor?> GetAsync(int id, CancellationToken cancellationToken = default);
        Task<Doctor> AddAsync(Doctor doctor, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(int id,Doctor doctor, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
