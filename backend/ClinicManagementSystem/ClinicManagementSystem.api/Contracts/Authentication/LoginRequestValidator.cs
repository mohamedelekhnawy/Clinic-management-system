using ClinicManagementSystem.api.Contracts.Authentication;

namespace ClinicManagementSystem.api.Contracts.Clinic
{
    public class LoginRequestValidator :AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator() {
            RuleFor(x => x.Email)
                .NotEmpty();
            RuleFor(x => x.Password)
                .NotEmpty();
        }
    }
}
