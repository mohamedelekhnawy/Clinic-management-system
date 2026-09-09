namespace ClinicManagementSystem.api.Services
{
    public interface IEmailService
    {
        Task SendVerificationCodeAsync(string email, string firstName, string code, CancellationToken cancellationToken = default);
    }
}
