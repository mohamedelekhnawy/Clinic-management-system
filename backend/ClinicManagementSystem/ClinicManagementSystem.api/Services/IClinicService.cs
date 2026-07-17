namespace ClinicManagementSystem.api.Services
{
    public interface IClinicService
    {
        Task<IEnumerable<Clinic>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Clinic?> GetAsync(int id, CancellationToken cancellationToken = default);
        Task<Clinic> AddAsync(Clinic clinic, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(int id, Clinic clinic,CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
